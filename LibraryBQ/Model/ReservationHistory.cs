using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class ReservationHistory
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public int UserId { get; set; }

    public DateOnly ReservationDate { get; set; }

    public DateOnly? ReservationDueDate { get; set; }

    public byte Priority { get; set; }

    public virtual BookCopy BookCopy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
