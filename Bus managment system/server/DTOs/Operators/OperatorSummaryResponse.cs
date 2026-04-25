namespace server.DTOs.Operators;

public class OperatorSummaryResponse
{
    public int TotalRoutes { get; set; }
    public int PendingRoutes { get; set; }
    public int ApprovedRoutes { get; set; }
    public int RejectedRoutes { get; set; }
    public int TotalBuses { get; set; }
    public int ActiveBuses { get; set; }
}
