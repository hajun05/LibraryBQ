using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class BookCopy
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int LoanStatusId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<LoanHistory> LoanHistories { get; set; } = new List<LoanHistory>();

    public virtual LoanStatus LoanStatus { get; set; } = null!;

    public virtual ICollection<ReservationHistory> ReservationHistories { get; set; } = new List<ReservationHistory>();
}
