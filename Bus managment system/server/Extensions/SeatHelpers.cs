using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Extensions;

public static class SeatHelpers
{
    public static async Task ReleaseExpiredSeatsAsync(AppDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expired = await db.Seats
            .Where(s => s.Status == SeatStatus.FROZEN && s.FreezeExpiresAt <= now)
            .ToListAsync(ct);

        if (expired.Count == 0)
        {
            return;
        }

        foreach (var seat in expired)
        {
            seat.Status = SeatStatus.AVAILABLE;
            seat.FreezeExpiresAt = null;
            seat.BookedByUserId = null;
        }

        await db.SaveChangesAsync(ct);
    }

    public static List<Seat> CreateDefaultSeats(Guid scheduleId)
    {
        var seats = new List<Seat>(40);
        var rows = new[] { "A", "B", "C", "D" };

        foreach (var row in rows)
        {
            for (var i = 1; i <= 10; i++)
            {
                seats.Add(new Seat
                {
                    SeatId = Guid.NewGuid(),
                    ScheduleId = scheduleId,
                    SeatNumber = $"{row}{i}",
                    Status = SeatStatus.AVAILABLE
                });
            }
        }

        return seats;
    }
}
