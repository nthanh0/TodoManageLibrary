using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class LoanSlip
{
    public string LoanId { get; set; } = null!;

    public string ReaderId { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;

    public DateOnly LoanDate { get; set; }

    public DateOnly? ExpiredDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string? Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<LoanDetail> LoanDetails { get; set; } = new List<LoanDetail>();

    public virtual Reader Reader { get; set; } = null!;
}
