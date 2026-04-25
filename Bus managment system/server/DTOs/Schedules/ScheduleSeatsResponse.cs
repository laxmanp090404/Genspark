namespace server.DTOs.Schedules;

public class ScheduleSeatsResponse
{
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string TravelDate { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal SeatPrice { get; set; }
    public List<SeatInfoResponse> Seats { get; set; } = new();
}
