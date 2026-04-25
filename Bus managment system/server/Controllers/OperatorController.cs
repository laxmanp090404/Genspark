using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Operators;
using server.Extensions;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/operator")]
[Authorize(Roles = "BUS_OPERATOR")]
public class OperatorController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var operatorId = User.GetUserId();

        var routes = db.Routes.Where(r => r.OperatorId == operatorId);
        var buses = db.Buses.Where(b => b.OperatorId == operatorId);

        var data = new OperatorSummaryResponse
        {
            TotalRoutes = await routes.CountAsync(ct),
            PendingRoutes = await routes.CountAsync(r => r.Status == RouteStatus.PENDING_APPROVAL, ct),
            ApprovedRoutes = await routes.CountAsync(r => r.Status == RouteStatus.APPROVED, ct),
            RejectedRoutes = await routes.CountAsync(r => r.Status == RouteStatus.REJECTED, ct),
            TotalBuses = await buses.CountAsync(ct),
            ActiveBuses = await buses.CountAsync(b => b.Status == BusStatus.ACTIVE, ct)
        };

        return Ok(ApiResponse<OperatorSummaryResponse>.Ok(data));
    }
}
