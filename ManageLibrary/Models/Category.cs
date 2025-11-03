using System;
using System.Collections.Generic;

namespace ManageLibrary.Models;

public partial class Category
{
    public string CategoryId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
