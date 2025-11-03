using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManageLibrary.Models;

namespace ManageLibrary.Controllers
{
    public class ReadersController : Controller
    {
        private readonly ManageLibraryContext _context;

        public ReadersController(ManageLibraryContext context)
        {
            _context = context;
        }

        // GET: Readers
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["CurrentFilter"] = search;

            var query = _context.Readers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(r =>
                    (r.ReaderId != null && r.ReaderId.ToLower().Contains(term)) ||
                    (r.FullName != null && r.FullName.ToLower().Contains(term)) ||
                    (r.Telephone != null && r.Telephone.ToLower().Contains(term)) ||
                    (r.Department != null && r.Department.ToLower().Contains(term))
                );
            }

            var readers = await query.AsNoTracking().ToListAsync();
            return View(readers);
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _context.Readers
                .FirstOrDefaultAsync(m => m.ReaderId == id);
            if (reader == null)
            {
                return NotFound();
            }

            return View(reader);
        }

        // GET: Readers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Readers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,DateOfBirth,NationalId,TypeOfReader,Email,Telephone,Address,Department")] Reader reader)
        {
            // === BƯỚC 1: SINH ID TRƯỚC ===
            string newReaderId;
            bool exists;
            var random = new Random();

            do
            {
                // Tạo 9 số ngẫu nhiên
                string msv = random.Next(100000000, 999999999).ToString();
                newReaderId = "DG" + msv;

                // Kiểm tra xem ID này đã tồn tại chưa
                exists = await _context.Readers.AnyAsync(r => r.ReaderId == newReaderId);
            } while (exists); // Lặp lại nếu ID đã tồn tại

            reader.ReaderId = newReaderId; // Gán ID mới cho model

            // === BƯỚC 2: XÓA LỖI VALIDATION CỦA READERID ===
            // Vì chúng ta đã tự gán ID, chúng ta cần báo cho ModelState biết
            // rằng trường ReaderId bây giờ đã hợp lệ (hoặc không cần kiểm tra nữa).
            ModelState.Remove(nameof(reader.ReaderId));

            // === BƯỚC 3: KIỂM TRA VALIDATE CHO CÁC TRƯỜNG CÒN LẠI ===
            if (ModelState.IsValid)
            {
                _context.Add(reader);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm độc giả thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState vẫn không hợp lệ (ví dụ: do FullName bị trống, hoặc SĐT sai định dạng)
            // nó sẽ quay về View và hiển thị lỗi cho các trường đó.
            return View(reader);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
            {
                return NotFound();
            }
            return View(reader);
        }

        // POST: Readers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ReaderId,FullName,DateOfBirth,NationalId,TypeOfReader,Email,Telephone,Address,Department")] Reader reader)
        {
            if (id != reader.ReaderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reader);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReaderExists(reader.ReaderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(reader);
        }

        // GET: Readers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _context.Readers
                .FirstOrDefaultAsync(m => m.ReaderId == id);
            if (reader == null)
            {
                return NotFound();
            }

            return View(reader);
        }

        // POST: Readers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // 1. KIỂM TRA ĐIỀU KIỆN: Độc giả có đang mượn sách không?
            // (Trạng thái "Đang mượn" hoặc bất kỳ trạng thái nào không phải "Đã trả")
            bool hasUnreturnedLoans = await _context.LoanSlips
                .AnyAsync(ls => ls.ReaderId == id && ls.Status != "Đã trả");

            if (hasUnreturnedLoans)
            {
                // 2. NẾU CÓ: Không xóa, gửi thông báo lỗi và quay lại
                TempData["ErrorMessage"] = "Không thể xóa. Độc giả này vẫn còn sách chưa trả.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // 3. NẾU KHÔNG CÒN MƯỢN: Tiến hành xóa toàn bộ lịch sử (logic cũ của bạn)
                // Sử dụng một transaction để đảm bảo tất cả các thao tác xóa
                // (Details, Slips, Account, Reader) đều thành công hoặc không gì cả.
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var reader = await _context.Readers.FindAsync(id);
                        if (reader == null)
                        {
                            return NotFound();
                        }

                        // 1. Xóa Tài khoản (Account) liên quan
                        var account = await _context.Accounts
                                            .FirstOrDefaultAsync(a => a.ReaderId == id);
                        if (account != null)
                        {
                            _context.Accounts.Remove(account);
                        }

                        // 2. Tìm tất cả Phiếu Mượn (LoanSlips) của độc giả này
                        // (Tại thời điểm này, nếu có, tất cả đều là phiếu "Đã trả")
                        var loanSlips = await _context.LoanSlips
                                              .Where(ls => ls.ReaderId == id)
                                              .ToListAsync();

                        if (loanSlips.Any())
                        {
                            // 3. Với mỗi Phiếu Mượn, tìm và xóa các Chi Tiết Mượn (LoanDetails)
                            foreach (var slip in loanSlips)
                            {
                                var loanDetails = await _context.LoanDetails
                                                        .Where(ld => ld.LoanId == slip.LoanId)
                                                        .ToListAsync();
                                if (loanDetails.Any())
                                {
                                    _context.LoanDetails.RemoveRange(loanDetails);
                                }
                            }

                            // 4. Sau khi xóa các chi tiết, xóa tất cả Phiếu Mượn (lịch sử)
                            _context.LoanSlips.RemoveRange(loanSlips);
                        }

                        // 5. Cuối cùng, xóa Độc Giả (Reader)
                        _context.Readers.Remove(reader);

                        // 6. Lưu tất cả thay đổi và commit transaction
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Đã xóa độc giả thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // Nếu có bất kỳ lỗi nào, rollback
                        await transaction.RollbackAsync();

                        TempData["ErrorMessage"] = "Xóa thất bại. Đã xảy ra lỗi: " + ex.Message;
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
        }

        private bool ReaderExists(string id)
        {
            return _context.Readers.Any(e => e.ReaderId == id);
        }
    }
}
