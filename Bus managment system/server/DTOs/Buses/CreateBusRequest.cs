using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Buses;

public class CreateBusRequest
{
    [Required]
    public Guid RouteId { get; set; }

    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Required]
    public TimeSpan ArrivalTime { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal SeatPrice { get; set; }
}
