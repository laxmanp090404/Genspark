using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class BookCategory
{
    public short CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Createdat { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
