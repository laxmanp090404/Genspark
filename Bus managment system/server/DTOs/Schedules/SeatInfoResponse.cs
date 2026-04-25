namespace server.DTOs.Schedules;

public class SeatInfoResponse
{
    public Guid SeatId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PassengerName { get; set; }
    public int? PassengerAge { get; set; }
    public string? PassengerGender { get; set; }
    public bool IsFrozenByCurrentUser { get; set; }
}
