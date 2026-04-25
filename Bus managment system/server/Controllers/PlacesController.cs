using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/places")]
public class PlacesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["q", "limit"])]
    public async Task<IActionResult> Get([FromQuery] string? q, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        if (limit <= 0 || limit > 500)
        {
            return BadRequest(ApiResponse<object>.Fail("limit must be between 1 and 500."));
        }

        var query = db.Routes
            .AsNoTracking()
            .Where(r => r.Status == RouteStatus.APPROVED)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(r => r.Source.ToLower().Contains(term) || r.Destination.ToLower().Contains(term));
        }

        var sources = await query.Select(r => r.Source).ToListAsync(ct);
        var destinations = await query.Select(r => r.Destination).ToListAsync(ct);

        var results = sources
            .Concat(destinations)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .Take(limit)
            .ToList();

        return Ok(ApiResponse<List<string>>.Ok(results));
    }
}
