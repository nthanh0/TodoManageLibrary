using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class Publisher
{
    public string PublisherId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Telephone { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
