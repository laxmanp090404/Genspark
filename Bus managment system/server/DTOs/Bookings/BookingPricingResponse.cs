namespace server.DTOs.Bookings;

public class BookingPricingResponse
{
    public Guid BookingId { get; set; }
    public int SeatCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFeeSnapshot { get; set; }
}
