using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class Book
{
    public int BookId { get; set; }

    // foreign key with category
    public short CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string? IsbnBase { get; set; }

    public DateTime Createdat { get; set; }
    // collection to store editions (navigation prop)    
    public virtual ICollection<BookEdition> BookEditions { get; set; } = new List<BookEdition>();
    //populate category in this field(navigation prop)
    public virtual BookCategory Category { get; set; } = null!;
}
