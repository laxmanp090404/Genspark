using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class BookEdition
{
    public int EditionId { get; set; }

    public int BookId { get; set; }

    public int EditionNumber { get; set; }

    public string? EditionLabel { get; set; }

    public string? Isbn { get; set; }

    public string? Publisher { get; set; }

    public short? PublishedYear { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
