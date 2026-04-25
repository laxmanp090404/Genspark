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
}
