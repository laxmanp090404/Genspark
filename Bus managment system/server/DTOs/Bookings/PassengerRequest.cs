using System.ComponentModel.DataAnnotations;
using server.Models;

namespace server.DTOs.Bookings;

public class PassengerRequest
{
    [Required]
    public Guid SeatId { get; set; }

    [Required]
    [MaxLength(150)]
    public string PassengerName { get; set; } = string.Empty;

    [Range(0, 120)]
    public int PassengerAge { get; set; }

    [Required]
    public Gender PassengerGender { get; set; }

    public bool IsPrimary { get; set; }
}
