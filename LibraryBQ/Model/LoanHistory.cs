using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class LoanHistory
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public int UserId { get; set; }

    public DateOnly LoanDate { get; set; }

    public DateOnly LoanDueDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public byte ExtensionCount { get; set; }

    public virtual BookCopy BookCopy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
