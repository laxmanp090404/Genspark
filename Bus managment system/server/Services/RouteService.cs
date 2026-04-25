using server.DTOs.Common;
using server.DTOs.Routes;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;
using Route = server.Models.Route;

namespace server.Services;

public class RouteService(IRouteRepository routeRepo, IUserRepository userRepo) : IRouteService
{
    public async Task<RouteResponse> CreateRouteAsync(Guid operatorId, CreateRouteRequest request, CancellationToken ct = default)
        => await CreateRouteInternalAsync(operatorId, request, RouteStatus.PENDING_APPROVAL, false, ct);

    public async Task<RouteResponse> CreateRouteAsAdminAsync(Guid adminId, CreateRouteRequest request, CancellationToken ct = default)
        => await CreateRouteInternalAsync(adminId, request, RouteStatus.APPROVED, true, ct);

    private async Task<RouteResponse> CreateRouteInternalAsync(Guid ownerId, CreateRouteRequest request, RouteStatus status, bool approved, CancellationToken ct)
    {
        var source = request.Source.Trim();
        var destination = request.Destination.Trim();

        if (source.Equals(destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Source and destination cannot be the same.");
        }

        var exists = await routeRepo.ExistsAsync(ownerId, source, destination, ct);
        if (exists)
        {
            throw new InvalidOperationException($"Route {source} -> {destination} already exists for this operator.");
        }

        var route = new Route
        {
            RouteId = Guid.NewGuid(),
            OperatorId = ownerId,
            Source = source,
            Destination = destination,
            Status = status,
            ApprovedBy = approved ? ownerId : null,
            ApprovedAt = approved ? DateTime.UtcNow : null,
            RejectionReason = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await routeRepo.AddAsync(route, ct);

        if (approved)
        {
            await TryApproveReverseRouteAsync(route, ownerId, ct);
        }
        else
        {
            _ = await userRepo.GetSingleAdminAsync(ct);
        }

        return ToRouteResponse(route);
    }

    public async Task<PagedResult<RouteResponse>> GetMyRoutesAsync(Guid operatorId, RouteStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await routeRepo.GetByOperatorAsync(operatorId, status, page, pageSize, ct);

        return new PagedResult<RouteResponse>
        {
            Items = paged.Items.Select(ToRouteResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResult<AdminRouteListItemResponse>> GetAdminRoutesAsync(RouteStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await routeRepo.GetForAdminAsync(status, page, pageSize, ct);

        return new PagedResult<AdminRouteListItemResponse>
        {
            Items = paged.Items.Select(r => new AdminRouteListItemResponse
            {
                RouteId = r.RouteId,
                OperatorId = r.OperatorId,
                OperatorName = r.Operator.Username,
                OperatorEmail = r.Operator.Email,
                Source = r.Source,
                Destination = r.Destination,
                Status = r.Status.ToString(),
                RejectionReason = r.RejectionReason,
                CreatedAt = r.CreatedAt
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<RouteResponse> ApproveRouteAsync(Guid adminId, Guid routeId, CancellationToken ct = default)
    {
        var route = await routeRepo.GetByIdAsync(routeId, ct)
            ?? throw new KeyNotFoundException("Route not found.");

        if (route.Status == RouteStatus.PENDING_DELETION)
        {
            var hasConfirmed = await routeRepo.HasConfirmedBookingsAsync(routeId, ct);
            if (hasConfirmed)
            {
                throw new InvalidOperationException("Cannot delete route with active or confirmed bookings.");
            }

            await routeRepo.DeleteRouteAndBusesAsync(route, ct);
            route.Status = RouteStatus.PENDING_DELETION;
            return ToRouteResponse(route);
        }

        if (route.Status != RouteStatus.PENDING_APPROVAL)
        {
            throw new InvalidOperationException("Route is not in PENDING_APPROVAL state.");
        }

        route.Status = RouteStatus.APPROVED;
        route.ApprovedBy = adminId;
        route.ApprovedAt = DateTime.UtcNow;
        route.RejectionReason = null;
        route.UpdatedAt = DateTime.UtcNow;

        await routeRepo.UpdateAsync(route, ct);
        await TryApproveReverseRouteAsync(route, adminId, ct);

        return ToRouteResponse(route);
    }

    public async Task<RouteResponse> RejectRouteAsync(Guid adminId, Guid routeId, string? reason, CancellationToken ct = default)
    {
        var route = await routeRepo.GetByIdAsync(routeId, ct)
            ?? throw new KeyNotFoundException("Route not found.");

        if (route.Status == RouteStatus.PENDING_DELETION)
        {
            route.Status = RouteStatus.APPROVED;
            route.UpdatedAt = DateTime.UtcNow;
            await routeRepo.UpdateAsync(route, ct);
            return ToRouteResponse(route);
        }

        if (route.Status != RouteStatus.PENDING_APPROVAL)
        {
            throw new InvalidOperationException("Route is not in PENDING_APPROVAL state.");
        }

        route.Status = RouteStatus.REJECTED;
        route.ApprovedBy = adminId;
        route.ApprovedAt = DateTime.UtcNow;
        route.RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        route.UpdatedAt = DateTime.UtcNow;

        await routeRepo.UpdateAsync(route, ct);

        return ToRouteResponse(route);
    }

    public async Task DeleteRouteAsync(Guid operatorId, Guid routeId, CancellationToken ct = default)
    {
        var route = await routeRepo.GetByIdForOperatorAsync(routeId, operatorId, ct)
            ?? throw new UnauthorizedAccessException("You do not own this route.");

        if (route.Status == RouteStatus.PENDING_DELETION)
        {
            throw new InvalidOperationException("Deletion request is already pending approval.");
        }

        if (route.Status != RouteStatus.APPROVED)
        {
            throw new InvalidOperationException("Only approved routes can be requested for deletion.");
        }

        route.Status = RouteStatus.PENDING_DELETION;
        route.UpdatedAt = DateTime.UtcNow;
        await routeRepo.UpdateAsync(route, ct);
    }

    private async Task TryApproveReverseRouteAsync(Route route, Guid approverId, CancellationToken ct)
    {
        var allRoutes = await routeRepo.GetByOperatorAsync(route.OperatorId, null, 1, int.MaxValue, ct);
        var reverseRoute = allRoutes.Items.FirstOrDefault(r =>
            r.RouteId != route.RouteId &&
            r.Source.Equals(route.Destination, StringComparison.OrdinalIgnoreCase) &&
            r.Destination.Equals(route.Source, StringComparison.OrdinalIgnoreCase));

        if (reverseRoute is null)
        {
            reverseRoute = new Route
            {
                RouteId = Guid.NewGuid(),
                OperatorId = route.OperatorId,
                Source = route.Destination,
                Destination = route.Source,
                Status = RouteStatus.APPROVED,
                ApprovedBy = approverId,
                ApprovedAt = DateTime.UtcNow,
                RejectionReason = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await routeRepo.AddAsync(reverseRoute, ct);
            return;
        }
        else if (reverseRoute.Status == RouteStatus.PENDING_APPROVAL || reverseRoute.Status == RouteStatus.REJECTED)
        {
            reverseRoute.Status = RouteStatus.APPROVED;
            reverseRoute.ApprovedBy = approverId;
            reverseRoute.ApprovedAt = DateTime.UtcNow;
            reverseRoute.RejectionReason = null;
            reverseRoute.UpdatedAt = DateTime.UtcNow;
            await routeRepo.UpdateAsync(reverseRoute, ct);
        }
    }

    private static RouteResponse ToRouteResponse(Route route) => new()
    {
        RouteId = route.RouteId,
        OperatorId = route.OperatorId,
        Source = route.Source,
        Destination = route.Destination,
        Status = route.Status.ToString(),
        ApprovedBy = route.ApprovedBy,
        ApprovedAt = route.ApprovedAt,
        RejectionReason = route.RejectionReason,
        CreatedAt = route.CreatedAt,
        BusCount = route.Buses.Count
    };
}
