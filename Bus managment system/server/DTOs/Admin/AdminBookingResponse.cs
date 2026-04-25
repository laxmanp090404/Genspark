using server.DTOs.Bookings;

namespace server.DTOs.Admin;

public class AdminBookingResponse : BookingSummaryResponse
{
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
