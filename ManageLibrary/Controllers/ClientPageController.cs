using Azure.Core;
using ManageLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
// Thêm namespace này để dùng ToListAsync()
using System.Linq;

namespace ManageLibrary.Controllers
{
    [Route("Home/")]
    public class ClientPageController : Controller
    {
        private readonly ManageLibraryContext _context;

        public ClientPageController(ManageLibraryContext context)
        {
            _context = context;
        }

        // === SỬA ĐỔI CHÍNH Ở ĐÂY ===
        // Chuyển sang async và truy vấn CSDL
        public async Task<IActionResult> Index()
        {
            // 1. Tải danh sách sách từ CSDL
            // Dùng Include() để tải kèm thông tin Tác giả và Thể loại
            var bookList = await _context.Books
                                .AsNoTracking()
                                .Include(b => b.Author)   // Tải kèm tác giả
                                .Include(b => b.Category) // Tải kèm thể loại
                                .OrderBy(b => b.Name)    // Sắp xếp theo tên
                                .ToListAsync();

            // 2. Tải dữ liệu cho các dropdown (nếu View của bạn có)
            await PopulateDropdowns();

            // 3. Gửi danh sách (bookList) làm Model sang View
            return View(bookList);
        }

        // (Hàm này giữ nguyên như code của bạn)
        private async Task PopulateDropdowns(string? selectedAuthorId = null, string? selectedPublisherId = null, string? selectedCategoryId = null)
        {
            // Tải danh sách Tác giả
            ViewBag.Authors = new SelectList(await _context.Authors.AsNoTracking().OrderBy(a => a.Name).ToListAsync(),
                "AuthorId", "Name", selectedAuthorId);

            // Tải danh sách Nhà xuất bản
            ViewBag.Publishers = new SelectList(await _context.Publishers.AsNoTracking().OrderBy(p => p.Name).ToListAsync(),
                "PublisherId", "Name", selectedPublisherId);

            // Tải danh sách Thể loại
            ViewBag.Categories = new SelectList(await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
                "CategoryId", "Name", selectedCategoryId);
        }
    }
}