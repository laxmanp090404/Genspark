using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class Borrowing
{
    public int BorrowingId { get; set; }

    public int MemberId { get; set; }

    public int CopyId { get; set; }

    public DateOnly BorrowedOn { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? ReturnedOn { get; set; }

    public string BorrowStatus { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public virtual BookCopy Copy { get; set; } = null!;

    public virtual Fine? Fine { get; set; }

    public virtual Member Member { get; set; } = null!;
}
