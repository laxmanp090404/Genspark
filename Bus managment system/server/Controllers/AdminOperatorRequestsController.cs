using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Admin;
using server.DTOs.Common;
using server.Extensions;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/admin/operator-requests")]
[Authorize(Roles = "ADMIN")]
public class AdminOperatorRequestsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] OperatorRequestStatus? status = OperatorRequestStatus.PENDING, CancellationToken ct = default)
    {
        var query = db.OperatorSwitchRequests
            .AsNoTracking()
            .Include(r => r.User)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(r => r.Status == status);
        }

        var items = await query
            .OrderBy(r => r.CreatedAt)
            .Select(r => new OperatorRequestResponse
            {
                RequestId = r.RequestId,
                UserId = r.UserId,
                Username = r.User.Username,
                Email = r.User.Email,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                ReviewedBy = r.ReviewedBy,
                ReviewedAt = r.ReviewedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<OperatorRequestResponse>>.Ok(items));
    }

    [HttpPatch("{requestId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid requestId, CancellationToken ct)
    {
        var adminId = User.GetUserId();

        var request = await db.OperatorSwitchRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.RequestId == requestId, ct);

        if (request is null)
        {
            return NotFound(ApiResponse<object>.Fail("Request not found."));
        }

        if (request.Status != OperatorRequestStatus.PENDING)
        {
            return BadRequest(ApiResponse<object>.Fail("Request is not in PENDING state."));
        }

        request.Status = OperatorRequestStatus.APPROVED;
        request.ReviewedBy = adminId;
        request.ReviewedAt = DateTime.UtcNow;

        request.User.Role = UserRole.BUS_OPERATOR;
        request.User.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null, "Operator request approved."));
    }

    [HttpPatch("{requestId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid requestId, CancellationToken ct)
    {
        var adminId = User.GetUserId();

        var request = await db.OperatorSwitchRequests
            .FirstOrDefaultAsync(r => r.RequestId == requestId, ct);

        if (request is null)
        {
            return NotFound(ApiResponse<object>.Fail("Request not found."));
        }

        if (request.Status != OperatorRequestStatus.PENDING)
        {
            return BadRequest(ApiResponse<object>.Fail("Request is not in PENDING state."));
        }

        request.Status = OperatorRequestStatus.REJECTED;
        request.ReviewedBy = adminId;
        request.ReviewedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null, "Operator request rejected."));
    }
}
