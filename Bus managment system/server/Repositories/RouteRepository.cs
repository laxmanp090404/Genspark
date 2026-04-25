using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.Models;
using server.Repositories.Interfaces;
using Route = server.Models.Route;

namespace server.Repositories;

public class RouteRepository(AppDbContext db) : IRouteRepository
{
    public Task<bool> ExistsAsync(Guid operatorId, string source, string destination, CancellationToken ct = default)
    {
        var src = source.Trim().ToLower();
        var dst = destination.Trim().ToLower();

        return db.Routes.AnyAsync(
            r => r.OperatorId == operatorId &&
                 r.Source.ToLower() == src &&
                 r.Destination.ToLower() == dst,
            ct);
    }

    public async Task AddAsync(Route route, CancellationToken ct = default)
    {
        await db.Routes.AddAsync(route, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<Route?> GetByIdAsync(Guid routeId, CancellationToken ct = default)
        => db.Routes
            .Include(r => r.Operator)
            .Include(r => r.Buses)
            .FirstOrDefaultAsync(r => r.RouteId == routeId, ct);

    public Task<Route?> GetByIdForOperatorAsync(Guid routeId, Guid operatorId, CancellationToken ct = default)
        => db.Routes
            .Include(r => r.Buses)
            .FirstOrDefaultAsync(r => r.RouteId == routeId && r.OperatorId == operatorId, ct);

    public async Task<PagedResult<Route>> GetByOperatorAsync(Guid operatorId, RouteStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Routes
            .Include(r => r.Buses)
            .Where(r => r.OperatorId == operatorId)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Route>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Route>> GetForAdminAsync(RouteStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Routes
            .Include(r => r.Operator)
            .Where(r => status == null || r.Status == status)
            .AsQueryable();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Route>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task UpdateAsync(Route route, CancellationToken ct = default)
    {
        db.Routes.Update(route);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> HasConfirmedBookingsAsync(Guid routeId, CancellationToken ct = default)
    {
        return db.Bookings.AnyAsync(
            b => b.Status == BookingStatus.CONFIRMED &&
                 b.Schedule.Bus.RouteId == routeId,
            ct);
    }

    public async Task DeleteRouteAndBusesAsync(Route route, CancellationToken ct = default)
    {
        foreach (var bus in route.Buses)
        {
            bus.Status = BusStatus.DELETED;
            bus.UpdatedAt = DateTime.UtcNow;
        }

        db.Routes.Remove(route);
        await db.SaveChangesAsync(ct);
    }
}
