using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Payments;
using server.Extensions;
using server.Models;

using Microsoft.AspNetCore.SignalR;

namespace server.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "USER")]
public class PaymentsController(AppDbContext db, server.Services.IEmailService emailService, IHubContext<server.Hubs.SeatHub> hubContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        await SeatHelpers.ReleaseExpiredSeatsAsync(db, ct);

        var userId = User.GetUserId();
        var booking = await db.Bookings
            .Include(b => b.BookingSeats)
            .ThenInclude(bs => bs.Seat)
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .Include(b => b.Payment)
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
            return BadRequest(ApiResponse<object>.Fail("No seats found for this booking."));
        }

        var now = DateTime.UtcNow;
        if (booking.BookingSeats.Any(bs => bs.Seat.Status != SeatStatus.FROZEN || bs.Seat.BookedByUserId != userId || bs.Seat.FreezeExpiresAt <= now))
        {
            return Conflict(ApiResponse<object>.Fail("Seat freeze has expired or is invalid."));
        }

        if (booking.Payment is not null && booking.Payment.Status == PaymentStatus.SUCCESS)
        {
            return Conflict(ApiResponse<object>.Fail("Payment already completed."));
        }

        decimal amount = booking.TotalAmount;
        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            var code = request.VoucherCode.Trim();
            var voucher = await db.DiscountVouchers
                .FirstOrDefaultAsync(v => v.Code.ToLower() == code.ToLower() && v.UserId == userId, ct);

            if (voucher is null)
            {
                return NotFound(ApiResponse<object>.Fail("Voucher not found."));
            }

            if (voucher.IsUsed)
            {
                return BadRequest(ApiResponse<object>.Fail("Voucher has already been used."));
            }

            if (voucher.ExpiresAt <= now)
            {
                return BadRequest(ApiResponse<object>.Fail("Voucher has expired."));
            }

            var discount = amount * (voucher.DiscountPercent / 100m);
            amount = Math.Max(0, amount - discount);
            voucher.IsUsed = true;
        }

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            BookingId = booking.BookingId,
            Amount = amount,
            Status = PaymentStatus.SUCCESS,
            PayerName = request.PayerName.Trim(),
            PayerEmail = request.PayerEmail.Trim().ToLowerInvariant(),
            TransactionRef = $"TXN-{Guid.NewGuid():N}",
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        booking.Status = BookingStatus.CONFIRMED;
        booking.TotalAmount = amount;
        booking.UpdatedAt = DateTime.UtcNow;

        foreach (var seat in booking.BookingSeats.Select(bs => bs.Seat))
        {
            seat.Status = SeatStatus.BOOKED;
            seat.FreezeExpiresAt = null;
            seat.BookedByUserId = userId;
        }

        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        var response = new PaymentResponse
        {
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            PaidAt = payment.PaidAt
        };

        await hubContext.Clients.Group(booking.ScheduleId.ToString()).SendAsync("SeatStatusChanged", ct);

        try
        {
            var passengersHtml = string.Join("", booking.BookingSeats.Select(bs => 
                $"<tr><td style='border: 1px solid #ccc; padding: 8px;'>{bs.Seat.SeatNumber}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerName}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerAge}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerGender}</td></tr>"));

            var emailBody = $@"
                <div style='font-family: sans-serif; color: #333;'>
                    <h2>Booking Confirmed!</h2>
                    <p>Dear {payment.PayerName},</p>
                    <p>Your payment of <strong>₹{payment.Amount}</strong> for booking <strong>{payment.BookingId}</strong> was successful.</p>
                    
                    <h3>Journey Details</h3>
                    <p><strong>Route:</strong> {booking.Schedule.Bus.Route.Source} to {booking.Schedule.Bus.Route.Destination}</p>
                    <p><strong>Date:</strong> {booking.Schedule.TravelDate:yyyy-MM-dd}</p>
                    <p><strong>Time:</strong> {booking.Schedule.Bus.DepartureTime} - {booking.Schedule.Bus.ArrivalTime}</p>
                    <p><strong>Bus:</strong> {booking.Schedule.Bus.RegistrationNumber}</p>

                    <h3>Passenger Details</h3>
                    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
                        <thead>
                            <tr style='background-color: #f8f9fa;'>
                                <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Seat</th>
                                <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Name</th>
                                <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Age</th>
                                <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Gender</th>
                            </tr>
                        </thead>
                        <tbody>
                            {passengersHtml}
                        </tbody>
                    </table>
                    <br/>
                    <p>Thank you for choosing Bus Management System.</p>
                </div>";

            await emailService.SendEmailAsync(payment.PayerEmail, payment.PayerName, "Booking Confirmed - Bus Management System", emailBody, ct);
        }
        catch (Exception ex)
        {
            // Log exception, but don't fail the payment
            Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
        }

        return Ok(ApiResponse<PaymentResponse>.Ok(response, "Payment successful."));
    }
}
