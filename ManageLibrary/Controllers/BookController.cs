using Azure.Core;
using ManageLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace QLThuVien.Controllers
{
    [Authorize] // Bảo vệ toàn bộ controller
    [Route("/Admin/Book")]
    public class BookController : Controller
    {
        private readonly ManageLibraryContext _context;

        public BookController(ManageLibraryContext context)
        {
            _context = context;
        }

        // ==============================================
        // HELPER: Tải Dropdown
        // ==============================================
        /// <summary>
        /// Tải dữ liệu cho các dropdown (Tác giả, NXB, Thể loại)
        /// </summary>
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

        // ==============================================
        // HELPER: Tạo mã sách mới
        // ==============================================
        /// <summary>
        /// Tự động tạo mã sách mới (VD: S020 -> S021)
        /// </summary>
        private async Task<string> GenerateNewBookIdAsync()
        {
            const string prefix = "S";
            const int paddingLength = 3; // Định dạng S001 (3 chữ số)

            // 1. Tìm sách có BookId lớn nhất
            var lastBook = await _context.Books
                .Where(b => b.BookId.StartsWith(prefix))
                .OrderByDescending(b => b.BookId) // Sắp xếp giảm dần
                .AsNoTracking()
                .FirstOrDefaultAsync();

            int nextNumber = 1; // Bắt đầu từ 1 nếu chưa có sách nào

            if (lastBook != null)
            {
                // 2. Lấy phần số từ ID (Bỏ qua prefix 'S')
                string numberPart = lastBook.BookId.Substring(prefix.Length);

                // 3. Chuyển sang số và + 1
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                // Nếu TryParse thất bại (VD: ID là 'S_ABC'), nó sẽ tự động dùng nextNumber = 1
            }

            // 4. Định dạng lại mã mới
            // "D3" đảm bảo số luôn có 3 chữ số, VD: 1 -> "001", 21 -> "021", 123 -> "123"
            return $"{prefix}{nextNumber:D3}";
        }
        // Đặt hàm này bên cạnh hàm GenerateNewBookIdAsync

        /// <summary>
        /// Tự động tạo mã Tác Giả mới (VD: TG001 -> TG002)
        /// </summary>
        private async Task<string> GenerateNewAuthorIdAsync()
        {
            const string prefix = "TG"; // "TG" = Tác Giả
            const int paddingLength = 3; // Định dạng TG001

            // 1. Tìm tác giả có AuthorId lớn nhất
            var lastAuthor = await _context.Authors
                .Where(a => a.AuthorId.StartsWith(prefix))
                .OrderByDescending(a => a.AuthorId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastAuthor != null)
            {
                // 2. Lấy phần số từ ID
                string numberPart = lastAuthor.AuthorId.Substring(prefix.Length);

                // 3. Chuyển sang số và + 1
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // 4. Định dạng lại mã mới
            return $"{prefix}{nextNumber:D3}";
        }

        // ==============================================
        // INDEX (GET)
        // ==============================================
        // GET: /Admin/Book
        [HttpGet] // Đây là action mặc định (không cần route con)
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Lưu lại từ khóa tìm kiếm để hiển thị lại trên View
            ViewData["CurrentFilter"] = searchString;

            // 2. Bắt đầu câu truy vấn (chưa thực thi)
            var booksQuery = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Category)
                .AsQueryable(); // Quan trọng: Bắt đầu với IQueryable

            // 3. Áp dụng bộ lọc nếu searchString có giá trị
            if (!String.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower();

                booksQuery = booksQuery.Where(b =>
                    b.Name.ToLower().Contains(searchLower) ||
                    (b.Author != null && b.Author.Name.ToLower().Contains(searchLower))
                );
            }

            // 4. Thực thi truy vấn (gọi ToListAsync) SAU KHI đã lọc
            var books = await booksQuery.AsNoTracking().ToListAsync();

            return View(books);
        }

        // ==============================================
        // ADD (GET)
        // ==============================================
        // GET: /Admin/Book/Add
        [HttpGet("Add")] // Sửa lỗi AmbiguousMatchException
        public async Task<IActionResult> Add()
        {
            // Tải các dropdown cho form Add
            await PopulateDropdowns();

            // *** THAY ĐỔI: Tạo một Book mới với ID đã được gán sẵn
            var newBook = new Book
            {
                BookId = await GenerateNewBookIdAsync(), // Tự động lấy mã mới
                Quantity = 1 // Giá trị mặc định cho số lượng
            };

            return View(newBook); // Trả về sách với ID đã điền
        }

        // ==============================================
        // ADD (POST)
        // ==============================================
        // POST: /Admin/Book/Add
        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            // Cập nhật [Bind] để bao gồm "Quantity"
            [Bind("BookId,Name,YearOfPublic,Position,NumOfPage,Cost,Quantity,CategoryId,AuthorId,PublisherId")] Book book,
            string NewAuthorName) // *** THAY ĐỔI: Thêm tham số "NewAuthorName"
        {
            // *** THAY ĐỔI: Xử lý logic tác giả MỚI trước khi validate
            if (!string.IsNullOrEmpty(NewAuthorName))
            {
                // Người dùng đã nhập tên tác giả mới -> Ưu tiên dùng tên này
                // 1. Kiểm tra xem tên tác giả này đã tồn tại trong DB chưa
                var trimmedName = NewAuthorName.Trim();
                var existingAuthor = await _context.Authors
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == trimmedName.ToLower());

                if (existingAuthor != null)
                {
                    // 2a. Nếu đã tồn tại, gán AuthorId của sách bằng ID của tác giả đó
                    book.AuthorId = existingAuthor.AuthorId;
                }
                else
                {
                    // 2b. Nếu CHƯA tồn tại, tạo tác giả mới
                    var newAuthor = new Author
                    {
                        AuthorId = await GenerateNewAuthorIdAsync(), // Dùng hàm mới tạo
                        Name = trimmedName
                        // Nếu Model Author có các trường [Required] khác, bạn cần thêm vào đây
                    };

                    _context.Authors.Add(newAuthor); // Thêm tác giả mới vào context

                    // Gán AuthorId của sách bằng ID TÁC GIẢ MỚI
                    book.AuthorId = newAuthor.AuthorId;
                }

                // 3. Vì ta đã gán `book.AuthorId` bằng tay,
                // ta cần xóa lỗi validation của trường AuthorId (nếu có)
                // (Trường hợp user không chọn gì từ dropdown, nó sẽ báo lỗi [Required])
                if (ModelState.ContainsKey(nameof(Book.AuthorId)))
                {
                    ModelState.Remove(nameof(Book.AuthorId));
                }
            }
            else if (string.IsNullOrEmpty(book.AuthorId))
            {
                // *** THAY ĐỔI: Báo lỗi nếu user không chọn dropdown VÀ cũng không nhập mới
                ModelState.AddModelError("AuthorId", "Bạn phải chọn một tác giả hoặc nhập tên tác giả mới.");
            }

            // -----------------------------------------------------------------
            // Code bên dưới gần như giữ nguyên

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(book); // Thêm SÁCH vào context

                    // *** QUAN TRỌNG ***
                    // Dòng này sẽ lưu CẢ SÁCH MỚI và TÁC GIẢ MỚI (nếu có) vào CSDL
                    // trong cùng 1 giao dịch (transaction).
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Giữ nguyên code bắt lỗi trùng khóa chính (PK)
                    if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        // Kiểm tra xem lỗi là của Sách hay Tác giả
                        if (ex.Message.Contains("Book"))
                        {
                            ModelState.AddModelError("BookId", $"Mã sách '{book.BookId}' đã tồn tại. Vui lòng tải lại trang để lấy mã mới.");
                        }
                        else if (ex.Message.Contains("Author"))
                        {
                            ModelState.AddModelError("AuthorId", $"Mã tác giả '{book.AuthorId}' đã tồn tại (lỗi trùng ID). Vui lòng thử lại.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Lỗi trùng lặp khóa chính.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng thử lại.");
                    }
                }
            }

            // Nếu ModelState không hợp lệ, tải lại dropdowns và hiển thị lại form
            await PopulateDropdowns(book.AuthorId, book.PublisherId, book.CategoryId);

            // *** THAY ĐỔI: Gửi lại tên tác giả mới mà user đã gõ
            ViewData["NewAuthorName"] = NewAuthorName;

            return View(book);
        }

        // ==============================================
        // EDIT (GET)
        // ==============================================
        // GET: /Admin/Book/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .AsNoTracking() // Dùng AsNoTracking() vì ta chỉ đọc dữ liệu để đổ vào form
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Tải dropdowns và chọn sẵn giá trị hiện tại của sách
            await PopulateDropdowns(book.AuthorId, book.PublisherId, book.CategoryId);
            return View(book);
        }

        // ==============================================
        // EDIT (POST)
        // ==============================================
        // POST: /Admin/Book/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,
            // Cập nhật [Bind] để bao gồm "Quantity"
            [Bind("BookId,Name,YearOfPublic,Position,NumOfPage,Cost,Quantity,CategoryId,AuthorId,PublisherId")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book); // Đánh dấu 'book' là đã bị sửa đổi
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Kiểm tra xem sách còn tồn tại không
                    if (!_context.Books.Any(e => e.BookId == book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Ném lỗi nếu có xung đột concurrency
                    }
                }
                return RedirectToAction(nameof(Index)); // Về trang danh sách
            }

            // Nếu ModelState không hợp lệ, tải lại dropdowns và hiển thị lại form
            await PopulateDropdowns(book.AuthorId, book.PublisherId, book.CategoryId);
            return View(book);
        }

        // ==============================================
        // DELETE (GET)
        // ==============================================
        // GET: /Admin/Book/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tải đầy đủ thông tin sách để hiển thị trang xác nhận xóa
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book); // Trả về view xác nhận xóa
        }

        // ==============================================
        // DELETE (POST)
        // ==============================================
        // POST: /Admin/Book/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")] // Khớp với form submit
        public async Task<IActionResult> DeleteConfirmed(string id)
       
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
    }
            return RedirectToAction(nameof(Index));
}
    }
}

