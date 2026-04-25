namespace server.DTOs.Bookings;

public class BookingSummaryResponse
{
    public Guid BookingId { get; set; }
    public Guid ScheduleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TravelDate { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string RefundStatus { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public List<string> SeatNumbers { get; set; } = new();
}
