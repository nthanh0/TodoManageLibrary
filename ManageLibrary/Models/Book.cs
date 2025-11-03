using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
// Thêm 2 thư viện để sử dụng Data Annotations
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageLibrary.Models;

public partial class Book
{
    // VARCHAR(20) PRIMARY KEY -> Required, StringLength(20)
    [Required(ErrorMessage = "Mã sách là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Mã sách không được vượt quá 20 ký tự.")]
    public string BookId { get; set; } = null!;

    // NVARCHAR(200) NOT NULL -> Required, StringLength(200)
    [Required(ErrorMessage = "Tên sách là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự.")]
    public string Name { get; set; } = null!;

    // INT (Nullable) -> Có thể thêm Range
    [Range(1000, 9999, ErrorMessage = "Năm xuất bản phải là một giá trị hợp lệ (ví dụ: 2024).")]
    public int? YearOfPublic { get; set; }

    // NVARCHAR(50) (Nullable)
    [StringLength(50, ErrorMessage = "Vị trí không được vượt quá 50 ký tự.")]
    public string? Position { get; set; }

    // INT (Nullable) -> Sách phải có ít nhất 1 trang
    // *** SỬA LỖI TYPO "1Setting" THÀNH "1" ***
    [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là một số dương.")]
    public int? NumOfPage { get; set; }

    // DECIMAL(10,2) (Nullable) -> Giá tiền không được âm
    [Column(TypeName = "decimal(10, 2)")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá tiền phải là một giá trị không âm.")]
    public decimal? Cost { get; set; }

    // *** THÊM LẠI TRƯỜNG QUANTITY ***
    // INT NOT NULL (Dựa trên script SQL đã chạy)
    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số không âm.")]
    public int Quantity { get; set; }

    // VARCHAR(20) (Nullable, FK)
    [StringLength(20)]
    public string? CategoryId { get; set; }

    // VARCHAR(20) (Nullable, FK)
    [StringLength(20)]
    public string? AuthorId { get; set; }

    // VARCHAR(20) (Nullable, FK)
    [StringLength(20)]
    public string? PublisherId { get; set; }

    // ================================================
    // Navigation Properties (Thuộc tính điều hướng)
    // ================================================

    [JsonIgnore] // Bỏ qua khi serialize JSON để tránh vòng lặp
    public virtual Author? Author { get; set; }

    [JsonIgnore]
    public virtual Category? Category { get; set; }

    [JsonIgnore]
    public virtual ICollection<LoanDetail> LoanDetails { get; set; } = new List<LoanDetail>();

    [JsonIgnore]
    public virtual Publisher? Publisher { get; set; }

    [JsonIgnore]
    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
}

