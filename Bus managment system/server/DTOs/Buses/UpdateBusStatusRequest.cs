using System.ComponentModel.DataAnnotations;
using server.Models;

namespace server.DTOs.Buses;

public class UpdateBusStatusRequest
{
    [Required]
    public BusStatus Status { get; set; }
}
