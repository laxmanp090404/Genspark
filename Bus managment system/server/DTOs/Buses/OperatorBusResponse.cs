namespace server.DTOs.Buses;

public class OperatorBusResponse
{
    public Guid BusId { get; set; }
    public Guid RouteId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal SeatPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}
