namespace server.DTOs.Config;

public class PlatformFeeResponse
{
    public decimal PlatformFee { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
