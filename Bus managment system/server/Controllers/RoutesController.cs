using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Common;
using server.DTOs.Routes;
using server.Extensions;
using server.Models;
using server.Services.Interfaces;

namespace server.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize(Roles = "BUS_OPERATOR")]
public class RoutesController(IRouteService routeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request, CancellationToken ct)
    {
        try
        {
            var operatorId = User.GetUserId();
            var route = await routeService.CreateRouteAsync(operatorId, request, ct);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<RouteResponse>.Ok(route, "Route submitted for admin approval."));
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
    public async Task<IActionResult> GetMyRoutes([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var operatorId = User.GetUserId();

        RouteStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RouteStatus>(status, true, out var parsed))
        {
            parsedStatus = parsed;
        }

        var data = await routeService.GetMyRoutesAsync(operatorId, parsedStatus, page, pageSize, ct);
        return Ok(ApiResponse<server.DTOs.Common.PagedResult<RouteResponse>>.Ok(data));
    }

    [HttpDelete("{routeId:guid}")]
    public async Task<IActionResult> DeleteRoute(Guid routeId, CancellationToken ct)
    {
        try
        {
            var operatorId = User.GetUserId();
            await routeService.DeleteRouteAsync(operatorId, routeId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Deletion request submitted for admin approval."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status409Conflict, ApiResponse<object>.Fail(ex.Message));
        }
    }
}
