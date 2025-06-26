using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
