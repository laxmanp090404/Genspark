using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class Fine
{
    public int FineId { get; set; }

    public int BorrowingId { get; set; }

    public int MemberId { get; set; }

    public short DaysOverdue { get; set; }

    public decimal FinePerDay { get; set; }

    public string FineStatus { get; set; } = null!;

    public string? WaiverReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Borrowing Borrowing { get; set; } = null!;

    public virtual FinePayment? FinePayment { get; set; }

    public virtual Member Member { get; set; } = null!;
}
