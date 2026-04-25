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
[Route("api/operator-requests")]
[Authorize(Roles = "USER")]
public class OperatorRequestsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var existingPending = await db.OperatorSwitchRequests.AnyAsync(
            r => r.UserId == userId && r.Status == OperatorRequestStatus.PENDING,
            ct);

        if (existingPending)
        {
            return Conflict(ApiResponse<object>.Fail("A pending request already exists."));
        }

        var request = new OperatorSwitchRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
            Status = OperatorRequestStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };

        db.OperatorSwitchRequests.Add(request);
        await db.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateOperatorRequestResponse>.Ok(new CreateOperatorRequestResponse
        {
            RequestId = request.RequestId,
            UserId = request.UserId,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt
        }, "Operator switch request submitted."));
    }

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var items = await db.OperatorSwitchRequests
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CreateOperatorRequestResponse
            {
                RequestId = r.RequestId,
                UserId = r.UserId,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<CreateOperatorRequestResponse>>.Ok(items));
    }
}
