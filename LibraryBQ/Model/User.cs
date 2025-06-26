using System;
using System.Collections.Generic;

namespace LibraryBQ.Model;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public byte MaxLoanNum { get; set; }

    public string UserNo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<LoanHistory> LoanHistories { get; set; } = new List<LoanHistory>();

    public virtual ICollection<ReservationHistory> ReservationHistories { get; set; } = new List<ReservationHistory>();
}
