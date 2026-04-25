using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Bookings;
using server.DTOs.Common;
using server.Extensions;
using server.Models;

using Microsoft.AspNetCore.SignalR;

namespace server.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "USER")]
public class BookingsController(AppDbContext db, server.Services.IEmailService emailService, IHubContext<server.Hubs.SeatHub> hubContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        await SeatHelpers.ReleaseExpiredSeatsAsync(db, ct);

        var userId = User.GetUserId();
        var booking = await db.Bookings
            .Include(b => b.BookingSeats)
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .ThenInclude(b => b.Route)
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId && b.UserId == userId, ct);

        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking not found."));
        }

        if (booking.Status != BookingStatus.PENDING)
        {
            return BadRequest(ApiResponse<object>.Fail("Booking is not in PENDING state."));
        }

        if (booking.BookingSeats.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("No frozen seats found for this booking."));
        }

        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToHashSet();
        if (request.Passengers.Count != seatIds.Count || request.Passengers.Any(p => !seatIds.Contains(p.SeatId)))
        {
            return BadRequest(ApiResponse<object>.Fail("Passenger seats do not match the frozen seats."));
        }

        var seats = await db.Seats.Where(s => seatIds.Contains(s.SeatId)).ToListAsync(ct);
        var now = DateTime.UtcNow;
        if (seats.Any(s => s.Status != SeatStatus.FROZEN || s.BookedByUserId != userId || s.FreezeExpiresAt <= now))
        {
            return Conflict(ApiResponse<object>.Fail("Seat freeze has expired or is invalid."));
        }

        foreach (var passenger in request.Passengers)
        {
            var bookingSeat = booking.BookingSeats.First(bs => bs.SeatId == passenger.SeatId);
            bookingSeat.PassengerName = passenger.PassengerName.Trim();
            bookingSeat.PassengerAge = passenger.PassengerAge;
            bookingSeat.PassengerGender = passenger.PassengerGender;
            bookingSeat.IsPrimary = passenger.IsPrimary;
        }

        var platformFee = await db.PlatformConfigs
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => c.PlatformFee)
            .FirstOrDefaultAsync(ct);

        var seatCount = booking.BookingSeats.Count;
        booking.PlatformFeeSnapshot = platformFee;
        booking.TotalAmount = (booking.Schedule.Bus.SeatPrice * seatCount) + platformFee;
        booking.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var response = new BookingPricingResponse
        {
            BookingId = booking.BookingId,
            SeatCount = seatCount,
            TotalAmount = booking.TotalAmount,
            PlatformFeeSnapshot = booking.PlatformFeeSnapshot
        };

        return Ok(ApiResponse<BookingPricingResponse>.Ok(response, "Booking details captured."));
    }

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var bookings = await db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .ThenInclude(b => b.Route)
            .Include(b => b.BookingSeats)
            .ThenInclude(bs => bs.Seat)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var items = bookings.Select(b => new BookingSummaryResponse
        {
            BookingId = b.BookingId,
            ScheduleId = b.ScheduleId,
            Status = b.Status.ToString(),
            TravelDate = b.Schedule.TravelDate.ToString("yyyy-MM-dd"),
            Source = b.Schedule.Bus.Route.Source,
            Destination = b.Schedule.Bus.Route.Destination,
            TotalAmount = b.TotalAmount,
            RefundStatus = b.RefundStatus.ToString(),
            RefundAmount = b.RefundAmount,
            SeatNumbers = b.BookingSeats.Select(bs => bs.Seat.SeatNumber).OrderBy(s => s).ToList()
        }).ToList();

        return Ok(ApiResponse<List<BookingSummaryResponse>>.Ok(items));
    }

    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> Get(Guid bookingId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings
            .AsNoTracking()
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .ThenInclude(b => b.Route)
            .Include(b => b.BookingSeats)
            .ThenInclude(bs => bs.Seat)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId, ct);

        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking not found."));
        }

        var passengers = booking.BookingSeats.Select(bs => new BookingPassengerResponse
        {
            SeatId = bs.SeatId,
            SeatCode = $"{booking.Schedule.Bus.RegistrationNumber}-{bs.Seat.SeatNumber}",
            SeatNumber = bs.Seat.SeatNumber,
            PassengerName = bs.PassengerName,
            PassengerAge = bs.PassengerAge,
            PassengerGender = bs.PassengerGender.ToString(),
            IsPrimary = bs.IsPrimary
        }).OrderBy(p => p.SeatNumber).ToList();

        var response = new BookingDetailResponse
        {
            BookingId = booking.BookingId,
            Status = booking.Status.ToString(),
            TravelDate = booking.Schedule.TravelDate.ToString("yyyy-MM-dd"),
            Source = booking.Schedule.Bus.Route.Source,
            Destination = booking.Schedule.Bus.Route.Destination,
            RegistrationNumber = booking.Schedule.Bus.RegistrationNumber,
            DepartureTime = booking.Schedule.Bus.DepartureTime,
            ArrivalTime = booking.Schedule.Bus.ArrivalTime,
            SeatPrice = booking.Schedule.Bus.SeatPrice,
            TotalAmount = booking.TotalAmount,
            PlatformFeeSnapshot = booking.PlatformFeeSnapshot,
            RefundStatus = booking.RefundStatus.ToString(),
            RefundAmount = booking.RefundAmount,
            SeatCodes = booking.BookingSeats.Select(bs => $"{booking.Schedule.Bus.RegistrationNumber}-{bs.Seat.SeatNumber}").OrderBy(s => s).ToList(),
            SeatNumbers = booking.BookingSeats.Select(bs => bs.Seat.SeatNumber).OrderBy(s => s).ToList(),
            Passengers = passengers
        };

        return Ok(ApiResponse<BookingDetailResponse>.Ok(response));
    }

    [HttpDelete("{bookingId:guid}")]
    public async Task<IActionResult> Cancel(Guid bookingId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .ThenInclude(b => b.Route)
            .Include(b => b.BookingSeats)
            .ThenInclude(bs => bs.Seat)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId, ct);

        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking not found."));
        }

        if (booking.Status != BookingStatus.CONFIRMED)
        {
            return BadRequest(ApiResponse<object>.Fail("Only confirmed bookings can be cancelled."));
        }

        var departureTime = TimeOnly.FromTimeSpan(booking.Schedule.Bus.DepartureTime);
        var departure = booking.Schedule.TravelDate.ToDateTime(departureTime);
        if (departure <= DateTime.UtcNow)
        {
            return BadRequest(ApiResponse<object>.Fail("Booking can no longer be cancelled."));
        }

        var hours = (departure - DateTime.UtcNow).TotalHours;
        decimal refundAmount;
        RefundStatus refundStatus;

        if (hours > 48)
        {
            refundAmount = booking.TotalAmount;
            refundStatus = RefundStatus.FULL;
        }
        else if (hours > 24)
        {
            refundAmount = booking.TotalAmount * 0.5m;
            refundStatus = RefundStatus.PARTIAL;
        }
        else
        {
            refundAmount = 0;
            refundStatus = RefundStatus.NONE;
        }

        booking.Status = BookingStatus.CANCELLED_BY_USER;
        booking.RefundStatus = refundStatus;
        booking.RefundAmount = refundAmount;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        foreach (var seat in booking.BookingSeats.Select(bs => bs.Seat))
        {
            seat.Status = SeatStatus.AVAILABLE;
            seat.FreezeExpiresAt = null;
            seat.BookedByUserId = null;
        }

        if (booking.Payment is not null && refundAmount > 0)
        {
            booking.Payment.Status = PaymentStatus.REFUNDED;
        }

        await db.SaveChangesAsync(ct);

        await hubContext.Clients.Group(booking.ScheduleId.ToString()).SendAsync("SeatStatusChanged", ct);

        if (booking.Payment is not null)
        {
            try
            {
                var emailBody = $@"
                    <div style='font-family: sans-serif; color: #333;'>
                        <h2>Booking Cancelled</h2>
                        <p>Dear {booking.Payment.PayerName},</p>
                        <p>Your booking <strong>{booking.BookingId}</strong> has been cancelled.</p>
                        
                        <h3>Refund Details</h3>
                        <p><strong>Refund Status:</strong> {refundStatus}</p>
                        <p><strong>Refund Amount:</strong> ₹{refundAmount}</p>
                        <br/>
                        <p>Thank you for choosing Bus Management System.</p>
                    </div>";

                await emailService.SendEmailAsync(booking.Payment.PayerEmail, booking.Payment.PayerName, "Booking Cancelled - Bus Management System", emailBody, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send cancellation email: {ex.Message}");
            }
        }

        return Ok(ApiResponse<object>.Ok(null, "Booking cancelled."));
    }
}
