namespace server.DTOs.Buses;

public class BusSearchItemResponse
{
    public Guid BusId { get; set; }
    public Guid? ScheduleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? TravelDate { get; set; }
    public int? AvailableSeats { get; set; }
    public decimal SeatPrice { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
