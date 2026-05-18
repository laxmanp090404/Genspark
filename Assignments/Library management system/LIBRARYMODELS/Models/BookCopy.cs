using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class BookCopy
{
    public int CopyId { get; set; }

    public int EditionId { get; set; }

    public string CopyStatus { get; set; } = null!;

    public DateOnly? AcquiredOn { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public virtual ICollection<BookDamageLog> BookDamageLogs { get; set; } = new List<BookDamageLog>();

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual BookEdition Edition { get; set; } = null!;
}
