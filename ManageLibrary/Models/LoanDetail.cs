using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class LoanDetail
{
    public string LoanDetailId { get; set; } = null!;

    public string LoanId { get; set; } = null!;

    public string BookId { get; set; } = null!;

    public string? LoanStatus { get; set; }

    public string? ReturnStatus { get; set; }

    public bool? IsLose { get; set; }

    public decimal? Fine { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual LoanSlip Loan { get; set; } = null!;
}
