using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Bookings;

public class CreateBookingRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    [MinLength(1)]
    public List<PassengerRequest> Passengers { get; set; } = new();
}
