using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class FinePayment
{
    public int PaymentId { get; set; }

    public int FineId { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime PaidOn { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Fine Fine { get; set; } = null!;
}
