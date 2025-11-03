using System;
using System.Collections.Generic;
// Thêm 2 thư viện để sử dụng Data Annotations
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Thêm thư viện để sử dụng JsonIgnore
using System.Text.Json.Serialization;


namespace ManageLibrary.Models;

public partial class Author
{
    // VARCHAR(20) PRIMARY KEY -> Required, StringLength(20)
    [Required(ErrorMessage = "Mã tác giả là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Mã tác giả không được vượt quá 20 ký tự.")]
    public string AuthorId { get; set; } = null!;

    // NVARCHAR(100) NOT NULL -> Required, StringLength(100)
    [Required(ErrorMessage = "Tên tác giả là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên tác giả không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = null!;

    // ================================================
    // Navigation Properties (Thuộc tính điều hướng)
    // ================================================

    // Thuộc tính này tương ứng với khóa ngoại AuthorId trong bảng Books (quan hệ 1-nhiều)
    [JsonIgnore]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    // Thuộc tính này (trong model Book là 'Authors') tương ứng với bảng 'BookAuthor' (quan hệ nhiều-nhiều)
    // Tên 'BooksNavigation' có thể được EF tự sinh ra
    [JsonIgnore]
    public virtual ICollection<Book> BooksNavigation { get; set; } = new List<Book>();
}
