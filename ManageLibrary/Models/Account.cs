using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class Account
{
    public string AccountId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? EmployeeId { get; set; }

    public string? ReaderId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Reader? Reader { get; set; }
}
