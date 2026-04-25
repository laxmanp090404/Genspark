using server.DTOs.Common;
using server.DTOs.Routes;
using server.Models;

namespace server.Services.Interfaces;

public interface IRouteService
{
    Task<RouteResponse> CreateRouteAsync(Guid operatorId, CreateRouteRequest request, CancellationToken ct = default);
    Task<RouteResponse> CreateRouteAsAdminAsync(Guid adminId, CreateRouteRequest request, CancellationToken ct = default);
    Task<PagedResult<RouteResponse>> GetMyRoutesAsync(Guid operatorId, RouteStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<AdminRouteListItemResponse>> GetAdminRoutesAsync(RouteStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<RouteResponse> ApproveRouteAsync(Guid adminId, Guid routeId, CancellationToken ct = default);
    Task<RouteResponse> RejectRouteAsync(Guid adminId, Guid routeId, string? reason, CancellationToken ct = default);
    Task DeleteRouteAsync(Guid operatorId, Guid routeId, CancellationToken ct = default);
}
