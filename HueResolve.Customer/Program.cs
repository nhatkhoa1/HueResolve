using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using HueResolve.Business.Services;

var builder = WebApplication.CreateBuilder(args);

/// 1. Cấu hình các dịch vụ MVC
builder.Services.AddControllersWithViews();

// Lưu Data Protection key vào thư mục cố định để tránh lỗi cookie sau mỗi lần restart
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".dp-keys")))
    .SetApplicationName("HueResolve.Customer");

/// 2. Cấu hình Xác thực (Authentication) bằng Cookie dành cho Người dân
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "HueResolve.Customer.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); /// Ghi nhớ đăng nhập 30 ngày
    });

var app = builder.Build();

/// 3. Lấy chuỗi kết nối từ appsettings.json
var connectionString = app.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

/// 4. KHỞI TẠO CÁC STATIC SERVICES (TẦNG BUSINESS)
/// Việc khởi tạo này đảm bảo các Repository bên trong Service có Connection String để làm việc với DB.
ReportService.Initialize(connectionString);
UserService.Initialize(connectionString);
CategoryService.Initialize(connectionString);
DepartmentService.Initialize(connectionString);
AssignmentService.Initialize(connectionString);
MapService.Initialize(connectionString);

/// 5. Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/// Kích hoạt xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

/// 6. Định nghĩa Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();