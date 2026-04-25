using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Routes;

public class CreateRouteRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public string Destination { get; set; } = string.Empty;
}
