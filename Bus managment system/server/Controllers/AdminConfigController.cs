using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Config;
using server.Extensions;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/admin/config")]
[Authorize(Roles = "ADMIN")]
public class AdminConfigController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var latest = await db.PlatformConfigs
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        if (latest is null)
        {
            return Ok(ApiResponse<PlatformFeeResponse>.Ok(new PlatformFeeResponse
            {
                PlatformFee = 0,
                UpdatedAt = DateTime.MinValue,
                UpdatedBy = null
            }));
        }

        return Ok(ApiResponse<PlatformFeeResponse>.Ok(new PlatformFeeResponse
        {
            PlatformFee = latest.PlatformFee,
            UpdatedAt = latest.UpdatedAt,
            UpdatedBy = latest.UpdatedBy
        }));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdatePlatformFeeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        var adminId = User.GetUserId();

        var config = new PlatformConfig
        {
            ConfigId = Guid.NewGuid(),
            PlatformFee = request.PlatformFee,
            UpdatedBy = adminId,
            UpdatedAt = DateTime.UtcNow
        };

        db.PlatformConfigs.Add(config);
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null, $"Platform fee updated to {request.PlatformFee}."));
    }
}
