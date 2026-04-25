using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Schedules;
using server.Extensions;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/schedules")]
[Authorize(Roles = "USER")]
public class SchedulesController(AppDbContext db) : ControllerBase
{
    [HttpGet("{scheduleId:guid}/seats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSeats(Guid scheduleId, CancellationToken ct)
    {
        await SeatHelpers.ReleaseExpiredSeatsAsync(db, ct);

        var currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;

        var schedule = await db.BusSchedules
            .AsNoTracking()
            .Include(s => s.Bus)
            .ThenInclude(b => b.Route)
            .Include(s => s.Seats)
            .ThenInclude(s => s.BookingSeats)
            .ThenInclude(bs => bs.Booking)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct);

        if (schedule is null)
        {
            return NotFound(ApiResponse<object>.Fail("Schedule not found."));
        }

        var bus = schedule.Bus;
        if (bus is null || bus.Route is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Schedule data is incomplete."));
        }

        var seats = schedule.Seats
            .OrderBy(s => s.SeatNumber)
            .Select(s =>
            {
                // Prioritize confirmed booking passenger info, then pending
                var activeBookingSeat = s.BookingSeats
                    .Where(bs => bs.Booking.Status == BookingStatus.CONFIRMED || bs.Booking.Status == BookingStatus.PENDING)
                    .OrderByDescending(bs => bs.Booking.CreatedAt)
                    .FirstOrDefault();

                var hasPassenger = (s.Status == SeatStatus.BOOKED || s.Status == SeatStatus.FROZEN) && activeBookingSeat is not null;

                return new SeatInfoResponse
                {
                    SeatId = s.SeatId,
                    SeatCode = $"{bus.RegistrationNumber}-{s.SeatNumber}",
                    SeatNumber = s.SeatNumber,
                    Status = s.Status.ToString(),
                    PassengerName = hasPassenger ? activeBookingSeat!.PassengerName : null,
                    PassengerAge = hasPassenger ? activeBookingSeat!.PassengerAge : null,
                    PassengerGender = hasPassenger ? activeBookingSeat!.PassengerGender.ToString() : null,
                    IsFrozenByCurrentUser = s.Status == SeatStatus.FROZEN && s.BookedByUserId == currentUserId
                };
            })
            .ToList();

        var response = new ScheduleSeatsResponse
        {
            ScheduleId = schedule.ScheduleId,
            BusId = schedule.BusId,
            RegistrationNumber = bus.RegistrationNumber,
            Source = bus.Route.Source,
            Destination = bus.Route.Destination,
            TravelDate = schedule.TravelDate.ToString("yyyy-MM-dd"),
            DepartureTime = bus.DepartureTime,
            ArrivalTime = bus.ArrivalTime,
            SeatPrice = bus.SeatPrice,
            Seats = seats
        };

        return Ok(ApiResponse<ScheduleSeatsResponse>.Ok(response));
    }
}
