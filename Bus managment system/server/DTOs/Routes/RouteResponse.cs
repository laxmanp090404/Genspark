namespace server.DTOs.Routes;

public class RouteResponse
{
    public Guid RouteId { get; set; }
    public Guid OperatorId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BusCount { get; set; }
}
