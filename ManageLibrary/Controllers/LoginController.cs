using ManageLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Linq;

namespace ManageLibrary.Controllers
{
    public class LoginController : Controller
    {
        // 1. Thêm DbContext
        private readonly ManageLibraryContext _context;

        public LoginController(ManageLibraryContext context)
        {
            _context = context;
        }

        // GET: /Login/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập, chuyển hướng tới trang admin
            if (User.Identity.IsAuthenticated)
            {
                // Bạn có thể phân quyền rõ hơn ở đây
                return Redirect("/Admin/Book");
            }
            return View();
        }

        // POST: /Login/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Account model)
        {
            if (model.Username == null || model.Password == null)
            {
                ViewBag.ErrorMessage = "Vui lòng nhập tài khoản và mật khẩu.";
                return View(model);
            }

            // 2. Tìm tài khoản trong CSDL
            var user = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            // 3. Kiểm tra logic đăng nhập
            if (user == null || user.Password != model.Password) // CẢNH BÁO BẢO MẬT: Xem ghi chú bên dưới
            {
                ViewBag.ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                return View(model);
            }

            // 4. Phân quyền VÀ Tạo "Phiên đăng nhập" (Cookie)
            var claims = new List<Claim>();
            string redirectUrl = "";

            if (user.EmployeeId != null) // A. Đây là tài khoản Admin (Employee)
            {
                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("AccountId", user.AccountId),
                    // Thêm EmployeeId vào claim nếu cần dùng
                    new Claim("EmployeeId", user.EmployeeId.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                redirectUrl = "/Admin/Book"; // Chuyển hướng đến trang Admin
            }
            else // B. Đây là tài khoản Client (Reader)
            {

                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("AccountId", user.AccountId),
                    // Thêm ReaderId vào claim nếu có và cần dùng, ví dụ:
                    // new Claim("ReaderId", user.ReaderId.ToString()), 
                    new Claim(ClaimTypes.Role, "Reader") // Thêm Role là "Reader"
                };
                redirectUrl = "Home/";
            }

            // 5. Tạo ClaimsIdentity và Đăng nhập (Dùng chung cho cả Admin và Reader)
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // Cấu hình thêm: cho phép ghi nhớ, thời gian hết hạn...
                // IsPersistent = true, 
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 6. Đăng nhập thành công, chuyển hướng
            return Redirect(redirectUrl);
        }

        // GET: /Login/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie đăng nhập
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login"); // Quay về trang đăng nhập
        }
    }
}