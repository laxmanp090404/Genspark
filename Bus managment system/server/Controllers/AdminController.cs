using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Admin;
using server.DTOs.Common;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var pendingRoutes = await db.Routes.CountAsync(r => r.Status == RouteStatus.PENDING_APPROVAL, ct);
        var pendingRequests = await db.OperatorSwitchRequests.CountAsync(r => r.Status == OperatorRequestStatus.PENDING, ct);
        var totalBookings = await db.Bookings.CountAsync(b => b.Status == BookingStatus.CONFIRMED, ct);
        var cancelledBookings = await db.Bookings.CountAsync(b => b.Status == BookingStatus.CANCELLED_BY_USER || b.Status == BookingStatus.CANCELLED_BY_SYSTEM, ct);

        var latestFee = await db.PlatformConfigs
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => c.PlatformFee)
            .FirstOrDefaultAsync(ct);

        var data = new AdminSummaryResponse
        {
            PendingRoutes = pendingRoutes,
            PendingOperatorRequests = pendingRequests,
            CurrentPlatformFee = latestFee,
            TotalBookings = totalBookings,
            CancelledBookings = cancelledBookings
        };

        return Ok(ApiResponse<AdminSummaryResponse>.Ok(data));
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings(CancellationToken ct)
    {
        var bookings = await db.Bookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Schedule)
            .ThenInclude(s => s.Bus)
            .ThenInclude(b => b.Route)
            .Include(b => b.BookingSeats)
            .ThenInclude(bs => bs.Seat)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var items = bookings.Select(b => new AdminBookingResponse
        {
            BookingId = b.BookingId,
            ScheduleId = b.ScheduleId,
            Status = b.Status.ToString(),
            TravelDate = b.Schedule.TravelDate.ToString("yyyy-MM-dd"),
            DepartureTime = b.Schedule.Bus.DepartureTime.ToString(),
            RegistrationNumber = b.Schedule.Bus.RegistrationNumber,
            Source = b.Schedule.Bus.Route.Source,
            Destination = b.Schedule.Bus.Route.Destination,
            TotalAmount = b.TotalAmount,
            RefundStatus = b.RefundStatus.ToString(),
            RefundAmount = b.RefundAmount,
            SeatNumbers = b.BookingSeats.Select(bs => bs.Seat.SeatNumber).OrderBy(s => s).ToList(),
            UserEmail = b.User.Email,
            UserName = b.User.Username,
            CreatedAt = b.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<AdminBookingResponse>>.Ok(items));
    }
}
