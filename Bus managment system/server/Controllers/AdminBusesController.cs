using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/admin/buses")]
[Authorize(Roles = "ADMIN")]
public class AdminBusesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var buses = await db.Buses
            .AsNoTracking()
            .Include(b => b.Route)
            .Include(b => b.Operator)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.BusId,
                OperatorName = b.Operator.Username,
                b.RegistrationNumber,
                Source = b.Route.Source,
                Destination = b.Route.Destination,
                b.DepartureTime,
                b.ArrivalTime,
                b.SeatPrice,
                Status = b.Status.ToString(),
                b.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(buses));
    }
}