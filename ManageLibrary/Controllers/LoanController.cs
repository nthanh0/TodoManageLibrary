using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManageLibrary.Models; // Dùng namespace model từ CSDL
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System; // Cần cho DateTime
using System.Collections.Generic; // Cần cho List

namespace QLThuVien.Controllers
{
    [Route("/Admin/Loan")]
    [Authorize] // 1. Bắt buộc đăng nhập để truy cập
    public class LoanController : Controller
    {
        // 2. Inject DbContext
        private readonly ManageLibraryContext _context;

        public LoanController(ManageLibraryContext context)
        {
            _context = context;
        }

        // 3. (Helper) Lấy EmployeeId của người đang đăng nhập
        private string GetCurrentEmployeeId()
        {
            // Chúng ta đã lưu EmployeeId vào Claim khi đăng nhập
            // (Nếu không tìm thấy, trả về "NV001" làm dự phòng)
            return User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value ?? "NV001";
        }

        // 4. (Helper) Tải Dropdowns
        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Readers = new SelectList(await _context.Readers.AsNoTracking().ToListAsync(), "ReaderId", "FullName");
            // *** SỬA: Chỉ hiển thị sách CÒN HÀNG (Quantity > 0) trong dropdown ***
            ViewBag.Books = new SelectList(
                await _context.Books
                    .Where(b => b.Quantity > 0) // Chỉ lấy sách còn hàng
                    .AsNoTracking()
                    .ToListAsync(),
                "BookId",
                "Name"
            );
        }


        // GET: /Admin/Loan
        public async Task<IActionResult> Index(string? search, string? status)
        {
            ViewData["CurrentFilter"] = search;
            ViewData["CurrentStatus"] = status;

            // Lấy dữ liệu từ DB, có lọc theo từ khóa và trạng thái
            var loansQuery = _context.LoanSlips
                .Include(l => l.Reader) // Nạp thông tin Độc giả
                .Include(l => l.Employee) // Nạp thông tin Nhân viên
                .Include(l => l.LoanDetails) // Nạp danh sách chi tiết
                    .ThenInclude(ld => ld.Book) // Nạp thông tin Sách từ chi tiết
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                loansQuery = loansQuery.Where(l =>
                    l.LoanId.ToLower().Contains(term) ||
                    (l.Reader != null && l.Reader.FullName.ToLower().Contains(term)) ||
                    l.ReaderId.ToLower().Contains(term)
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                loansQuery = loansQuery.Where(l => l.Status == status);
            }

            var loans = await loansQuery
                .AsNoTracking()
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            return View(loans);
        }

        // GET: /Admin/Loan/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync(); // Tải dropdowns

            var model = new LoanSlip
            {
                LoanDate = DateOnly.FromDateTime(DateTime.Today),
                ExpiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14)) // Hạn 14 ngày
            };
            return View(model);
        }

        // POST: /Admin/Loan/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanSlip loan, string[] selectedBooks)
        {
            // *** BẮT ĐẦU SỬA: Thêm logic kiểm tra số lượng ***

            // 1. Kiểm tra sách đã chọn (giữ nguyên)
            if (selectedBooks == null || !selectedBooks.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một cuốn sách.");
            }

            // 2. Kiểm tra độc giả (giữ nguyên)
            var readerExists = await _context.Readers.AnyAsync(r => r.ReaderId == loan.ReaderId);
            if (!readerExists)
            {
                ModelState.AddModelError(nameof(loan.ReaderId), "Độc giả không tồn tại.");
            }

            // 3. (MỚI) Kiểm tra số lượng sách TRƯỚC KHI tạo phiếu
            var booksToLoan = new List<Book>();
            if (selectedBooks != null)
            {
                foreach (var bookId in selectedBooks)
                {
                    var book = await _context.Books.FindAsync(bookId);
                    if (book == null)
                    {
                        ModelState.AddModelError("", $"Sách với ID {bookId} không tồn tại.");
                    }
                    else if (book.Quantity <= 0)
                    {
                        ModelState.AddModelError("", $"Sách '{book.Name}' (ID: {bookId}) đã hết hàng.");
                    }
                    else
                    {
                        booksToLoan.Add(book); // Sách hợp lệ, thêm vào danh sách
                    }
                }
            }

            // 4. Gán EmployeeId và Xóa lỗi ModelState (giữ nguyên)
            loan.EmployeeId = GetCurrentEmployeeId();
            ModelState.Remove(nameof(loan.LoanId));
            ModelState.Remove(nameof(loan.EmployeeId));
            ModelState.Remove(nameof(loan.Employee));
            ModelState.Remove(nameof(loan.Reader));


            if (ModelState.IsValid)
            {
                // 5. (MỚI) Bọc toàn bộ logic lưu CSDL trong Transaction
                // Nếu 1 bước lỗi (VD: Cập nhật Quantity lỗi), phiếu mượn sẽ bị hủy
                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 6. Gán thông tin phiếu mượn
                        loan.LoanId = "PM-" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                        loan.Status = "Đang mượn";

                        // 7. Thêm chi tiết và GIẢM SỐ LƯỢNG sách
                        foreach (var book in booksToLoan) // Dùng list sách đã kiểm tra
                        {
                            loan.LoanDetails.Add(new LoanDetail
                            {
                                LoanDetailId = loan.LoanId + "-" + book.BookId,
                                LoanId = loan.LoanId,
                                BookId = book.BookId,
                                LoanStatus = "Tốt", // Hoặc "Bình thường"
                                IsLose = false,
                                Fine = 0
                            });

                            // *** LOGIC MỚI: Trừ số lượng sách ***
                            book.Quantity -= 1;
                            _context.Update(book);
                        }

                        // 8. Lưu phiếu mượn VÀ lưu thay đổi số lượng
                        _context.LoanSlips.Add(loan);
                        await _context.SaveChangesAsync();

                        // 9. (MỚI) Nếu thành công, commit transaction
                        await transaction.CommitAsync();

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        // 10. (MỚI) Nếu lỗi, rollback
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo phiếu mượn: " + ex.Message);
                    }
                }
            }

            // Nếu vẫn còn lỗi, trả về view
            await PopulateDropdownsAsync();
            return View(loan);
        }

        // GET: /Admin/Loan/Return/{id}
        [HttpGet("Return/{id}")]
        public async Task<IActionResult> Return(string id)
        {
            if (id == null) return NotFound();

            var loan = await _context.LoanSlips
                .Include(l => l.Reader)
                .Include(l => l.LoanDetails)
                    .ThenInclude(ld => ld.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null) return NotFound();

            if (loan.Status == "Đã trả")
            {
                return RedirectToAction("Index");
            }

            return View(loan);
        }

        // POST: /Admin/Loan/Return/{id}
        [HttpPost("Return/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(string id, List<LoanDetail> loanDetails)
        {
            // *** BẮT ĐẦU SỬA: Thêm logic cộng lại số lượng ***

            // 1. (MỚI) Bọc trong Transaction
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 2. Tải phiếu mượn gốc từ CSDL
                    var loan = await _context.LoanSlips
                        .Include(l => l.LoanDetails) // Phải Include chi tiết
                        .FirstOrDefaultAsync(l => l.LoanId == id);

                    if (loan == null) return NotFound();

                    // 3. Cập nhật từng chi tiết
                    foreach (var submittedDetail in loanDetails)
                    {
                        var detailInDb = loan.LoanDetails
                            .FirstOrDefault(d => d.LoanDetailId == submittedDetail.LoanDetailId);

                        if (detailInDb != null)
                        {
                            detailInDb.ReturnStatus = submittedDetail.ReturnStatus;
                            detailInDb.IsLose = submittedDetail.IsLose;
                            detailInDb.Fine = submittedDetail.Fine;

                            // *** LOGIC MỚI: Cộng lại số lượng nếu sách KHÔNG MẤT ***
                            if (detailInDb.IsLose == false)
                            {
                                var bookToReturn = await _context.Books.FindAsync(detailInDb.BookId);
                                if (bookToReturn != null)
                                {
                                    bookToReturn.Quantity += 1;
                                    _context.Update(bookToReturn);
                                }
                            }
                            // (Nếu IsLose == true, chúng ta không cộng lại số lượng)
                        }
                    }

                    // 4. Cập nhật phiếu mượn
                    loan.ReturnDate = DateOnly.FromDateTime(DateTime.Today);
                    loan.Status = "Đã trả";

                    // 5. Lưu thay đổi (cho cả phiếu mượn, chi tiết, VÀ số lượng sách)
                    await _context.SaveChangesAsync();

                    // 6. (MỚI) Commit transaction
                    await transaction.CommitAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // 7. (MỚI) Rollback nếu lỗi
                    await transaction.RollbackAsync();
                    // (Cần tải lại dữ liệu cho View nếu muốn hiển thị lỗi)
                    ModelState.AddModelError("", "Lỗi khi trả sách: " + ex.Message);
                    // Tải lại dữ liệu để hiển thị lại trang Return
                    var loanForView = await _context.LoanSlips
                        .Include(l => l.Reader)
                        .Include(l => l.LoanDetails.Where(ld => ld.LoanId == id))
                            .ThenInclude(ld => ld.Book)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(l => l.LoanId == id);

                    return View(loanForView);
                }
            }
        }

        // POST: /Admin/Loan/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            // Chỉ cho xóa phiếu "Đã trả"
            var loan = await _context.LoanSlips
                .Include(l => l.LoanDetails)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
            {
                TempData["ErrorMessage"] = "Phiếu mượn không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (loan.Status != "Đã trả")
            {
                TempData["ErrorMessage"] = "Chỉ được xóa phiếu mượn đã trả.";
                return RedirectToAction(nameof(Index));
            }

            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa chi tiết trước
                    if (loan.LoanDetails != null && loan.LoanDetails.Any())
                    {
                        _context.LoanDetails.RemoveRange(loan.LoanDetails);
                    }

                    // Xóa phiếu mượn
                    _context.LoanSlips.Remove(loan);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Đã xóa phiếu mượn thành công.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Xóa phiếu mượn thất bại: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
