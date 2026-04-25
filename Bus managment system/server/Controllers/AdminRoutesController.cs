using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Common;
using server.DTOs.Routes;
using server.Extensions;
using server.Models;
using server.Services.Interfaces;

namespace server.Controllers;

[ApiController]
[Route("api/admin/routes")]
[Authorize(Roles = "ADMIN")]
public class AdminRoutesController(IRouteService routeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request, CancellationToken ct)
    {
        try
        {
            var adminId = User.GetUserId();
            var route = await routeService.CreateRouteAsAdminAsync(adminId, request, ct);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<RouteResponse>.Ok(route, "Route added and approved."));
        }
        catch (InvalidOperationException ex)
        {
            var code = ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return StatusCode(code, ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        RouteStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RouteStatus>(status, true, out var parsed))
        {
            parsedStatus = parsed;
        }

        var data = await routeService.GetAdminRoutesAsync(parsedStatus, page, pageSize, ct);
        return Ok(ApiResponse<server.DTOs.Common.PagedResult<AdminRouteListItemResponse>>.Ok(data));
    }

    [HttpPatch("{routeId:guid}/approve")]
    public async Task<IActionResult> ApproveRoute(Guid routeId, CancellationToken ct)
    {
        try
        {
            var adminId = User.GetUserId();
            var data = await routeService.ApproveRouteAsync(adminId, routeId, ct);
            return Ok(ApiResponse<RouteResponse>.Ok(data, "Route approved."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPatch("{routeId:guid}/reject")]
    public async Task<IActionResult> RejectRoute(Guid routeId, [FromBody] ApproveRejectRouteRequest request, CancellationToken ct)
    {
        try
        {
            var adminId = User.GetUserId();
            var data = await routeService.RejectRouteAsync(adminId, routeId, request.Reason, ct);
            return Ok(ApiResponse<RouteResponse>.Ok(data, "Route rejected."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
