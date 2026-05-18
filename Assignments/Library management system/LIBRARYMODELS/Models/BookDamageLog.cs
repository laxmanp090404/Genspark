using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class BookDamageLog
{
    public int DamageLogId { get; set; }

    public int CopyId { get; set; }

    public int? MemberId { get; set; }

    public DateOnly ReportedOn { get; set; }

    public string DamageDescription { get; set; } = null!;

    public string DamageSeverity { get; set; } = null!;

    public string ResultingStatus { get; set; } = null!;

    public decimal? FineApplied { get; set; }

    public DateTime Createdat { get; set; }

    public virtual BookCopy Copy { get; set; } = null!;

    public virtual Member? Member { get; set; }
}
