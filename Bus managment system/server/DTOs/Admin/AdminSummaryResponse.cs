namespace server.DTOs.Admin;

public class AdminSummaryResponse
{
    public int PendingRoutes { get; set; }
    public int PendingOperatorRequests { get; set; }
    public decimal CurrentPlatformFee { get; set; }
    public int TotalBookings { get; set; }
    public int CancelledBookings { get; set; }
}
