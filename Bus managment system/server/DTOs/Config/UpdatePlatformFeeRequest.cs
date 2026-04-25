using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Config;

public class UpdatePlatformFeeRequest
{
    [Range(0, double.MaxValue)]
    public decimal PlatformFee { get; set; }
}
