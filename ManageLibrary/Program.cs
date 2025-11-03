using ManageLibrary.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ManageLibraryContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("manageLibraryContext")));
// THÊM 2 DÒNG NÀY để cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Đường dẫn tới trang đăng nhập
        options.AccessDeniedPath = "/Login/AccessDenied"; // (Tùy chọn) Trang từ chối truy cập
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
// THÊM 2 DÒNG NÀY (Thứ tự RẤT quan trọng)
app.UseAuthentication(); // 1. Xác thực
app.UseAuthorization();  // 2. Phân quyền

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
