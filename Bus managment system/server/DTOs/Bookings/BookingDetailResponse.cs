namespace server.DTOs.Bookings;

public class BookingDetailResponse
{
    public Guid BookingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TravelDate { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal SeatPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFeeSnapshot { get; set; }
    public string RefundStatus { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public List<string> SeatCodes { get; set; } = new();
    public List<string> SeatNumbers { get; set; } = new();
    public List<BookingPassengerResponse> Passengers { get; set; } = new();
}
