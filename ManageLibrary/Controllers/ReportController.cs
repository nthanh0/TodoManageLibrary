using Microsoft.AspNetCore.Mvc;
using ManageLibrary.Models; // Sử dụng Model
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using System.Collections.Generic; // Cần cho List

namespace QLThuVien.Controllers
{
    [Route("/Admin/Report")]
    [Authorize] // Bắt buộc đăng nhập
    public class ReportController : Controller
    {
        private readonly ManageLibraryContext _context;

        public ReportController(ManageLibraryContext context)
        {
            _context = context;
        }

        // GET: /Admin/Report/Index
        [HttpGet] // <-- Thêm [HttpGet] để rõ ràng hơn
        public async Task<IActionResult> Index()
        {
            // 1. Tổng số Sách (ĐÃ SỬA: Dùng Sum(Quantity) theo yêu cầu của bạn)
            int totalBooks = await _context.Books.SumAsync(b => b.Quantity);

            // 2. Tổng số Độc giả
            int totalReaders = await _context.Readers.CountAsync();

            // 3. Số phiếu đang mượn
            int currentLoans = await _context.LoanSlips
                .CountAsync(l => l.Status == "Đang mượn");

            // 4. Số phiếu quá hạn (Logic của bạn rất tốt)
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int overdueLoans = await _context.LoanSlips
                .CountAsync(l => (l.Status == "Quá hạn")
                                  || (l.Status == "Đang mượn" && l.ExpiredDate != null && l.ExpiredDate < today));

            // 5. Top 10 sách mượn nhiều nhất (Dùng logic Join của bạn)
            var topLoanedBooks = await _context.LoanDetails
                .GroupBy(ld => ld.BookId) // Nhóm theo BookId
                .Select(g => new {
                    BookId = g.Key,
                    LoanCount = g.Count() // Đếm số lần mượn
                })
                .OrderByDescending(g => g.LoanCount) // Sắp xếp
                .Take(10) // Lấy 10
                .Join( // Kết nối với bảng Books để lấy Tên sách
                    _context.Books,
                    stats => stats.BookId,
                    book => book.BookId,
                    (stats, book) => new BookStatsViewModel
                    {
                        BookName = book.Name,
                        LoanCount = stats.LoanCount
                    })
                .ToListAsync();

            // 6. Thống kê Sách theo Thể loại
            var booksByCategory = await _context.Books
                .Include(b => b.Category) // Phải Include Category
                .GroupBy(b => b.Category.Name) // Nhóm theo Tên thể loại
                .Select(g => new CategoryStatsViewModel
                {
                    CategoryName = g.Key ?? "Chưa phân loại",
                    // ĐÃ SỬA: Dùng Sum(Quantity) theo yêuG cầu
                    BookCount = g.Sum(b => b.Quantity)
                })
                .OrderByDescending(c => c.BookCount)
                .ToListAsync();

            // 7. Tạo ViewModel để gửi sang View
            var viewModel = new ReportViewModel
            {
                TotalBooks = totalBooks,
                TotalReaders = totalReaders,
                CurrentLoans = currentLoans,
                OverdueLoans = overdueLoans,
                TopLoanedBooks = topLoanedBooks,
                BooksByCategory = booksByCategory
            };

            return View(viewModel);
        }
    }
}
