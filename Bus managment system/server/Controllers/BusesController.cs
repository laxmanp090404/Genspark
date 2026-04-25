using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using server.Data;
using server.DTOs.Buses;
using server.DTOs.Common;
using server.Extensions;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController(AppDbContext db, server.Services.IEmailService emailService, IHubContext<server.Hubs.SeatHub> hubContext) : ControllerBase
{
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? source, [FromQuery] string? destination, [FromQuery] string? date, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(date))
        {
            return BadRequest(ApiResponse<object>.Fail("source, destination and date are required."));
        }

        if (!DateOnly.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var travelDate))
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid travel date. Use yyyy-MM-dd."));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (travelDate < today)
        {
            return BadRequest(ApiResponse<object>.Fail("Travel date cannot be in the past."));
        }

        await SeatHelpers.ReleaseExpiredSeatsAsync(db, ct);

        var buses = await db.Buses
            .AsNoTracking()
            .Include(b => b.Route)
            .Include(b => b.Operator)
            .Where(b => b.Status == BusStatus.ACTIVE && b.Route.Status == RouteStatus.APPROVED)
            .ToListAsync(ct);

        var src = NormalizeSearchTerm(source);
        var dst = NormalizeSearchTerm(destination);

        var filteredBuses = buses
            .Select(b => new
            {
                Bus = b,
                Score = GetSearchScore(b.Route.Source, b.Route.Destination, src, dst)
                
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Bus.Route.Source)
            .ThenBy(x => x.Bus.Route.Destination)
            .Select(x => x.Bus)
            .ToList();
            
        Dictionary<Guid, BusSchedule> scheduleByBus = new();
        
        if (filteredBuses.Count > 0)
        {
            var busIds = filteredBuses.Select(b => b.BusId).ToList();
            
            var schedules = await db.BusSchedules
                .Include(s => s.Seats)
                .Where(s => busIds.Contains(s.BusId) && s.TravelDate == travelDate)
                .ToListAsync(ct);

            scheduleByBus = schedules.ToDictionary(s => s.BusId, s => s);

            var missingBusIds = busIds.Except(scheduleByBus.Keys).ToList();
            if (missingBusIds.Count > 0)
            {
                var newSchedules = new List<BusSchedule>();
                var newSeats = new List<Seat>();

                foreach (var busId in missingBusIds)
                {
                    var schedule = new BusSchedule
                    {
                        ScheduleId = Guid.NewGuid(),
                        BusId = busId,
                        TravelDate = travelDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    newSchedules.Add(schedule);
                    newSeats.AddRange(SeatHelpers.CreateDefaultSeats(schedule.ScheduleId));
                    scheduleByBus[busId] = schedule;
                }

                db.BusSchedules.AddRange(newSchedules);
                db.Seats.AddRange(newSeats);
                await db.SaveChangesAsync(ct);

                foreach (var schedule in newSchedules)
                {
                    schedule.Seats = newSeats.Where(s => s.ScheduleId == schedule.ScheduleId).ToList();
                }
            }
        }

        var items = filteredBuses.Select(b =>
        {
            
            scheduleByBus.TryGetValue(b.BusId, out var schedule);
            var availableSeats = schedule?.Seats.Count(s => s.Status == SeatStatus.AVAILABLE);

            return new BusSearchItemResponse
            {
                BusId = b.BusId,
                ScheduleId = schedule?.ScheduleId,
                RegistrationNumber = b.RegistrationNumber,
                OperatorName = b.Operator.Username,
                Source = b.Route.Source,
                Destination = b.Route.Destination,
                TravelDate = travelDate.ToString("yyyy-MM-dd"),
                AvailableSeats = schedule is null ? null : availableSeats,
                SeatPrice = b.SeatPrice,
                DepartureTime = b.DepartureTime,
                ArrivalTime = b.ArrivalTime,
                Status = b.Status.ToString()
            };
        }).ToList();

        return Ok(ApiResponse<List<BusSearchItemResponse>>.Ok(items));
    }

    // private static string NormalizeSearchTerm(string? value)
    //     => new((value ?? string.Empty).Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    private static string NormalizeSearchTerm(string? value)
    => (value ?? string.Empty).Trim().ToLowerInvariant();
    private static int GetSearchScore(string source, string destination, string sourceQuery, string destinationQuery)
    {
        var sourceScore = SimilarityScore(source, sourceQuery);
        var destinationScore = SimilarityScore(destination, destinationQuery);

        if (sourceScore <= 0 && destinationScore <= 0)
          return 0;

        return (sourceScore * 2) + destinationScore;
    }

    private static int SimilarityScore(string value, string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return 0;

        var normalizedValue = NormalizeSearchTerm(value);
        var normalizedQuery = NormalizeSearchTerm(query);

        if (normalizedValue == normalizedQuery) return 100;
        if (normalizedValue.StartsWith(normalizedQuery)) return 90;
        if (normalizedValue.Contains(normalizedQuery)) return 80;

        var distance = LevenshteinDistance(normalizedValue, normalizedQuery);
        var maxLength = Math.Max(normalizedValue.Length, normalizedQuery.Length);
        if (maxLength == 0) return 0;

        var score = (int)Math.Round((1d - (double)distance / maxLength) * 100d);
        return score >= 60 ? score : 0; // Increased threshold for better matches
    }

    private static int LevenshteinDistance(string left, string right)
    {
        if (left.Length == 0) return right.Length;
        if (right.Length == 0) return left.Length;

        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];

        for (var j = 0; j <= right.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= right.Length; j++)
            {
                var cost = left[i - 1] == right[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    [HttpGet]
    [Authorize(Roles = "BUS_OPERATOR")]
    public async Task<IActionResult> GetOwn([FromQuery] BusStatus? status, CancellationToken ct)
    {
        var operatorId = User.GetUserId();

        var query = db.Buses
            .AsNoTracking()
            .Include(b => b.Route)
            .Where(b => b.OperatorId == operatorId)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(b => b.Status == status);
        }

        var buses = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new OperatorBusResponse
            {
                BusId = b.BusId,
                RouteId = b.RouteId,
                RegistrationNumber = b.RegistrationNumber,
                Source = b.Route.Source,
                Destination = b.Route.Destination,
                DepartureTime = b.DepartureTime,
                ArrivalTime = b.ArrivalTime,
                SeatPrice = b.SeatPrice,
                Status = b.Status.ToString()
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<OperatorBusResponse>>.Ok(buses));
    }

    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAdminAll(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var buses = await db.Buses
            .AsNoTracking()
            .Include(b => b.Route)
            .Include(b => b.Operator)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var busIds = buses.Select(b => b.BusId).ToList();
        var schedules = await db.BusSchedules
            .Where(s => busIds.Contains(s.BusId) && s.TravelDate == today)
            .ToListAsync(ct);

        var scheduleMap = schedules.ToDictionary(s => s.BusId, s => s.ScheduleId);

        var items = buses.Select(b => new 
        {
            BusId = b.BusId,
            ScheduleId = scheduleMap.GetValueOrDefault(b.BusId),
            OperatorName = b.Operator.Username,
            RegistrationNumber = b.RegistrationNumber,
            Source = b.Route.Source,
            Destination = b.Route.Destination,
            DepartureTime = b.DepartureTime,
            ArrivalTime = b.ArrivalTime,
            SeatPrice = b.SeatPrice,
            Status = b.Status.ToString()
        }).ToList();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost]
    [Authorize(Roles = "BUS_OPERATOR")]
    public async Task<IActionResult> Create([FromBody] CreateBusRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        var operatorId = User.GetUserId();

        var route = await db.Routes.FirstOrDefaultAsync(r => r.RouteId == request.RouteId, ct);
        if (route is null)
        {
            return NotFound(ApiResponse<object>.Fail("Route not found."));
        }

        // Shared Routes: Removed operator ownership check here
        // if (route.OperatorId != operatorId)
        // {
        //     return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("You do not own this route."));
        // }

        if (route.Status != RouteStatus.APPROVED)
        {
            return BadRequest(ApiResponse<object>.Fail("Bus can only be created for approved routes."));
        }

        if (request.ArrivalTime <= request.DepartureTime)
        {
            return BadRequest(ApiResponse<object>.Fail("Arrival time must be after departure time."));
        }

        var regNo = request.RegistrationNumber.Trim();
        var exists = await db.Buses.AnyAsync(b => b.RegistrationNumber.ToLower() == regNo.ToLower(), ct);
        if (exists)
        {
            return Conflict(ApiResponse<object>.Fail("Registration number already exists."));
        }

        var bus = new Bus
        {
            BusId = Guid.NewGuid(),
            OperatorId = operatorId,
            RouteId = request.RouteId,
            RegistrationNumber = regNo,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            SeatPrice = request.SeatPrice,
            Status = BusStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Buses.Add(bus);
        await db.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Ok(new { busId = bus.BusId }, "Bus created successfully."));
    }

    [HttpPatch("{busId:guid}/status")]
    [Authorize(Roles = "BUS_OPERATOR")]
    public async Task<IActionResult> UpdateStatus(Guid busId, [FromBody] UpdateBusStatusRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        var operatorId = User.GetUserId();
        var bus = await db.Buses.FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId, ct);
        if (bus is null)
        {
            return NotFound(ApiResponse<object>.Fail("Bus not found."));
        }

        if (bus.Status == BusStatus.DELETED)
        {
            return BadRequest(ApiResponse<object>.Fail("Cannot update status of a deleted bus."));
        }

        bus.Status = request.Status;
        bus.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        // Send email to all users with active bookings on this bus
        try
        {
            var activeBookings = await db.Bookings
                .Include(b => b.Payment)
                .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
                .Include(b => b.Schedule)
                .ThenInclude(s => s.Bus)
                .ThenInclude(b => b.Route)
                .Where(b => b.Schedule.BusId == busId && b.Status == BookingStatus.CONFIRMED && b.Payment != null)
                .ToListAsync(ct);

            foreach (var booking in activeBookings)
            {
                var isCancelledByOperator = request.Status == BusStatus.OUT_OF_SERVICE;
                
                if (isCancelledByOperator)
                {
                    booking.Status = BookingStatus.CANCELLED_BY_SYSTEM;
                    booking.RefundStatus = RefundStatus.FULL;
                    booking.RefundAmount = booking.TotalAmount;
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;

                    if (booking.Payment != null)
                    {
                        booking.Payment.Status = PaymentStatus.REFUNDED;
                    }

                    foreach (var seat in booking.BookingSeats.Select(bs => bs.Seat))
                    {
                        seat.Status = SeatStatus.AVAILABLE;
                        seat.FreezeExpiresAt = null;
                        seat.BookedByUserId = null;
                    }
                }

                var passengersHtml = string.Join("", booking.BookingSeats.Select(bs => 
                    $"<tr><td style='border: 1px solid #ccc; padding: 8px;'>{bs.Seat.SeatNumber}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerName}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerAge}</td><td style='border: 1px solid #ccc; padding: 8px;'>{bs.PassengerGender}</td></tr>"));

                var subject = isCancelledByOperator ? "Bus Journey Cancelled - Refund Initiated" : "Bus Status Update - Bus Management System";
                var title = isCancelledByOperator ? "Journey Cancelled" : "Bus Status Update";
                var message = isCancelledByOperator 
                    ? $"We regret to inform you that your journey on bus <strong>{bus.RegistrationNumber}</strong> has been cancelled by the operator due to the bus being taken out of service."
                    : $"The status of your scheduled bus (<strong>{bus.RegistrationNumber}</strong>) has been updated to: <span style='color: #e67e22; font-weight: bold;'>{request.Status}</span>.";

                var emailBody = $@"
                    <div style='font-family: sans-serif; color: #333;'>
                        <h2 style='color: {(isCancelledByOperator ? "#c0392b" : "#2c3e50")};'>{title}</h2>
                        <p>Dear {booking.Payment?.PayerName ?? "Valued Customer"},</p>
                        <p>{message}</p>
                        
                        <h3>Journey Details</h3>
                        <p><strong>Route:</strong> {booking.Schedule.Bus?.Route?.Source ?? "N/A"} to {booking.Schedule.Bus?.Route?.Destination ?? "N/A"}</p>
                        <p><strong>Date:</strong> {booking.Schedule.TravelDate:yyyy-MM-dd}</p>
                        <p><strong>Departure:</strong> {booking.Schedule.Bus?.DepartureTime.ToString() ?? "N/A"}</p>

                        <h3>Passenger Details</h3>
                        <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
                            <thead>
                                <tr style='background-color: #f8f9fa;'>
                                    <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Seat</th>
                                    <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Name</th>
                                    <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Age</th>
                                    <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Gender</th>
                                </tr>
                            </thead>
                            <tbody>
                                {passengersHtml}
                            </tbody>
                        </table>

                        {(isCancelledByOperator ? $@"
                        <h3>Refund Information</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #c0392b;'>
                            <p><strong>Refund Status:</strong> FULL</p>
                            <p><strong>Refund Amount:</strong> ₹{booking.RefundAmount}</p>
                            <p>The amount will be credited to your original payment method within 3-5 business days.</p>
                        </div>" : "")}
                        
                        <p>If you have any questions, please contact our support team.</p>
                        <br/>
                        <p>Thank you for choosing Bus Management System.</p>
                    </div>";

                await emailService.SendEmailAsync(booking.Payment?.PayerEmail ?? "", booking.Payment?.PayerName ?? "Customer", subject, emailBody, ct);
            }

            await db.SaveChangesAsync(ct);
            await hubContext.Clients.All.SendAsync("SeatStatusChanged", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process bus status update: {ex.Message}");
        }

        return Ok(ApiResponse<object>.Ok(null, "Bus status updated."));
    }

    [HttpDelete("{busId:guid}")]
    [Authorize(Roles = "BUS_OPERATOR")]
    public async Task<IActionResult> Delete(Guid busId, CancellationToken ct)
    {
        var operatorId = User.GetUserId();
        var bus = await db.Buses.FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId, ct);
        if (bus is null)
        {
            return NotFound(ApiResponse<object>.Fail("Bus not found."));
        }

        bus.Status = BusStatus.DELETED;
        bus.UpdatedAt = DateTime.UtcNow;
        
        // Notify users
        var activeBookings = await db.Bookings
            .Include(b => b.Payment)
            .Include(b => b.Schedule)
            .Where(b => b.Schedule.BusId == busId && b.Status == BookingStatus.CONFIRMED && b.Payment != null)
            .ToListAsync(ct);

        // Process refunds automatically for deleted buses
        foreach (var booking in activeBookings)
        {
            booking.Status = BookingStatus.CANCELLED_BY_SYSTEM;
            booking.RefundStatus = RefundStatus.FULL;
            booking.RefundAmount = booking.TotalAmount;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            
            if (booking.Payment != null)
            {
                booking.Payment.Status = PaymentStatus.REFUNDED;
            }
        }
        
        await db.SaveChangesAsync(ct);

        try
        {
            foreach (var booking in activeBookings)
            {
                var emailBody = $@"
                    <div style='font-family: sans-serif; color: #333;'>
                        <h2 style='color: #c0392b;'>Bus Journey Cancelled</h2>
                        <p>Dear {booking.Payment?.PayerName ?? "Valued Customer"},</p>
                        <p>We regret to inform you that your scheduled journey on bus <strong>{bus.RegistrationNumber}</strong> has been cancelled by the operator.</p>
                        
                        <h3>Journey Details</h3>
                        <p><strong>Route:</strong> {booking.Schedule.Bus?.Route?.Source ?? "N/A"} to {booking.Schedule.Bus?.Route?.Destination ?? "N/A"}</p>
                        <p><strong>Date:</strong> {booking.Schedule.TravelDate:yyyy-MM-dd}</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #c0392b;'>
                            <h4 style='margin-top: 0;'>Refund Information</h4>
                            <p><strong>Refund Amount:</strong> ₹{booking.TotalAmount}</p>
                            <p><strong>Status:</strong> Full Refund Initiated</p>
                        </div>
                        
                        <p>The refund will be credited to your original payment method within 3-5 business days.</p>
                        <br/>
                        <p>We apologize for the inconvenience caused.</p>
                    </div>";

                await emailService.SendEmailAsync(booking.Payment?.PayerEmail ?? "", booking.Payment?.PayerName ?? "Customer", "URGENT: Bus Cancelled - Refund Initiated", emailBody, ct);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send bus deletion emails: {ex.Message}");
        }

        return Ok(ApiResponse<object>.Ok(null, "Bus deleted successfully."));
    }
}
