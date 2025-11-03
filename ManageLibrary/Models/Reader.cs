using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // <-- BẠN PHẢI THÊM DÒNG NÀY

namespace ManageLibrary.Models;

public partial class Reader
{
    public string ReaderId { get; set; } = null!; // Id này được tạo tự động nên không cần validate

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Ngày sinh")]
    [Required(ErrorMessage = "Ngày sinh không được để trống.")]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "CCCD/CMND")]
    [Required(ErrorMessage = "Số CCCD/CMND không được để trống.")]
    [StringLength(12, MinimumLength = 9, ErrorMessage = "CCCD/CMND phải có từ 9 đến 12 ký tự.")]
    public string? NationalId { get; set; }

    [Display(Name = "Loại độc giả")]
    [Required(ErrorMessage = "Vui lòng chọn loại độc giả.")]
    public string? TypeOfReader { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email không được để trống.")]
    // Dòng dưới đây bắt buộc phải có @ và kết thúc bằng .com
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$", ErrorMessage = "Email phải có định dạng hợp lệ (ví dụ: user@example.com).")]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    // Cái này không bắt buộc, nhưng nếu nhập thì phải đúng định dạng
    [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Telephone { get; set; }

    [Display(Name = "Địa chỉ")]
    [Required(ErrorMessage = "Địa chỉ không được để trống.")]
    public string? Address { get; set; }

    [Display(Name = "Khoa/Phòng ban")]
    public string? Department { get; set; } // Trường này có thể không bắt buộc

    public virtual Account? Account { get; set; }

    public virtual ICollection<LoanSlip> LoanSlips { get; set; } = new List<LoanSlip>();
}