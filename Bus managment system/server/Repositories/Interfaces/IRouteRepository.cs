using server.DTOs.Common;
using server.Models;
using Route = server.Models.Route;

namespace server.Repositories.Interfaces;

public interface IRouteRepository
{
    Task<bool> ExistsAsync(Guid operatorId, string source, string destination, CancellationToken ct = default);
    Task AddAsync(Route route, CancellationToken ct = default);
    Task<Route?> GetByIdAsync(Guid routeId, CancellationToken ct = default);
    Task<Route?> GetByIdForOperatorAsync(Guid routeId, Guid operatorId, CancellationToken ct = default);
    Task<PagedResult<Route>> GetByOperatorAsync(Guid operatorId, RouteStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Route>> GetForAdminAsync(RouteStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task UpdateAsync(Route route, CancellationToken ct = default);
    Task<bool> HasConfirmedBookingsAsync(Guid routeId, CancellationToken ct = default);
    Task DeleteRouteAndBusesAsync(Route route, CancellationToken ct = default);
}
