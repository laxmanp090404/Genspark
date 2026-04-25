using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Payments;

public class CreatePaymentRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    [MaxLength(150)]
    public string PayerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PayerEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? VoucherCode { get; set; }
}
