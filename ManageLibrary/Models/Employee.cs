using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class Employee
{
    public string EmployeeId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telephone { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<LoanSlip> LoanSlips { get; set; } = new List<LoanSlip>();
}
