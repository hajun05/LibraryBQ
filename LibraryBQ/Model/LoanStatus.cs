using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class LoanStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
