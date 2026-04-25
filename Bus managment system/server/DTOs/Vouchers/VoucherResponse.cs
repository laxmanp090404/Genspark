namespace server.DTOs.Vouchers;

public class VoucherResponse
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
}
