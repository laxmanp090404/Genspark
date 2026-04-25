using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Seats;
using server.Extensions;
using server.Models;

using Microsoft.AspNetCore.SignalR;

namespace server.Controllers;

[ApiController]
[Route("api/seats")]
[Authorize(Roles = "USER")]
public class SeatsController(AppDbContext db, IHubContext<server.Hubs.SeatHub> hubContext) : ControllerBase
{
    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze([FromBody] FreezeSeatsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        await SeatHelpers.ReleaseExpiredSeatsAsync(db, ct);

        var schedule = await db.BusSchedules
            .Include(s => s.Bus)
            .ThenInclude(b => b.Route)
            .FirstOrDefaultAsync(s => s.ScheduleId == request.ScheduleId, ct);

        if (schedule is null)
        {
            return NotFound(ApiResponse<object>.Fail("Schedule not found."));
        }

        if (schedule.Bus.Status != BusStatus.ACTIVE || schedule.Bus.Route.Status != RouteStatus.APPROVED)
        {
            return BadRequest(ApiResponse<object>.Fail("Bus is not available for booking."));
        }

        var userId = User.GetUserId();
        var seatIds = request.SeatIds.Distinct().ToList();

        var seats = await db.Seats
            .Where(s => s.ScheduleId == schedule.ScheduleId && seatIds.Contains(s.SeatId))
            .ToListAsync(ct);

        if (seats.Count != seatIds.Count)
        {
            return NotFound(ApiResponse<object>.Fail("One or more seats were not found."));
        }

        if (seats.Any(s => s.Status != SeatStatus.AVAILABLE))
        {
            return Conflict(ApiResponse<object>.Fail("Selected seats are not available."));
        }

        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            UserId = userId,
            ScheduleId = schedule.ScheduleId,
            Status = BookingStatus.PENDING,
            RefundStatus = RefundStatus.NOT_APPLICABLE,
            TotalAmount = 0,
            PlatformFeeSnapshot = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        foreach (var seat in seats)
        {
            seat.Status = SeatStatus.FROZEN;
            seat.FreezeExpiresAt = expiresAt;
            seat.BookedByUserId = userId;
        }

        var bookingSeats = seats.Select((seat, index) => new BookingSeat
        {
            BookingSeatId = Guid.NewGuid(),
            BookingId = booking.BookingId,
            SeatId = seat.SeatId,
            PassengerName = string.Empty,
            PassengerAge = 0,
            PassengerGender = Gender.OTHER,
            IsPrimary = index == 0
        }).ToList();

        db.Bookings.Add(booking);
        db.BookingSeats.AddRange(bookingSeats);
        await db.SaveChangesAsync(ct);

        await hubContext.Clients.Group(schedule.ScheduleId.ToString()).SendAsync("SeatStatusChanged", ct);

        var response = new FreezeSeatsResponse
        {
            BookingId = booking.BookingId,
            ScheduleId = schedule.ScheduleId,
            ExpiresAtUtc = expiresAt,
            SeatIds = seats.Select(s => s.SeatId).ToList()
        };

        return StatusCode(StatusCodes.Status201Created, ApiResponse<FreezeSeatsResponse>.Ok(response, "Seats frozen for 5 minutes."));
    }

    [HttpDelete("freeze/{bookingId:guid}")]
    public async Task<IActionResult> Release(Guid bookingId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings
            .Include(b => b.BookingSeats)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId, ct);

        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking not found."));
        }

        if (booking.Status != BookingStatus.PENDING)
        {
            return BadRequest(ApiResponse<object>.Fail("Only pending bookings can be released."));
        }

        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
        if (seatIds.Count > 0)
        {
            var seats = await db.Seats.Where(s => seatIds.Contains(s.SeatId)).ToListAsync(ct);
            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.AVAILABLE;
                seat.FreezeExpiresAt = null;
                seat.BookedByUserId = null;
            }
        }

        booking.Status = BookingStatus.CANCELLED_BY_USER;
        booking.RefundStatus = RefundStatus.NOT_APPLICABLE;
        booking.RefundAmount = null;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await hubContext.Clients.Group(booking.ScheduleId.ToString()).SendAsync("SeatStatusChanged", ct);

        return Ok(ApiResponse<object>.Ok(null, "Seat freeze released."));
    }
}
