using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Seats;

public class FreezeSeatsRequest
{
    [Required]
    public Guid ScheduleId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> SeatIds { get; set; } = new();
}
