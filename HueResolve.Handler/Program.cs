using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using HueResolve.Business.Services;

var builder = WebApplication.CreateBuilder(args);

/// Cấu hình dịch vụ MVC (Controllers và Views).
builder.Services.AddControllersWithViews();

// Lưu Data Protection key vào thư mục cố định để tránh lỗi cookie sau mỗi lần restart
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".dp-keys")))
    .SetApplicationName("HueResolve.Handler");

/// Lấy chuỗi kết nối từ cấu hình appsettings.json.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

/// Khởi tạo các dịch vụ Business tĩnh (Static Services) với chuỗi kết nối Database.
ReportService.Initialize(connectionString);
UserService.Initialize(connectionString);
DepartmentService.Initialize(connectionString);
CategoryService.Initialize(connectionString);
AssignmentService.Initialize(connectionString);

/// Cấu hình xác thực người dùng bằng Cookie (Authentication).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "HueResolve.Handler.Auth";
    });

var app = builder.Build();

/// Cấu hình đường ống xử lý yêu cầu HTTP (Middleware).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/// Thứ tự quan trọng: Authentication phải chạy trước Authorization.
app.UseAuthentication();
app.UseAuthorization();

/// Định nghĩa Route mặc định, ưu tiên trang Dashboard khi đã đăng nhập.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();