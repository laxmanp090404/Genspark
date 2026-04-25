namespace server.DTOs.Bookings;

public class BookingPassengerResponse
{
    public Guid SeatId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public int PassengerAge { get; set; }
    public string PassengerGender { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
