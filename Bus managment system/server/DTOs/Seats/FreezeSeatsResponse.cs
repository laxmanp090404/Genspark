namespace server.DTOs.Seats;

public class FreezeSeatsResponse
{
    public Guid BookingId { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
}
