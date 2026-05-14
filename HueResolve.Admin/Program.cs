using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using HueResolve.Business.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Lưu Data Protection key vào thư mục cố định để tránh lỗi cookie sau mỗi lần restart
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".dp-keys")))
    .SetApplicationName("HueResolve.Admin");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// ====================== KHỞI TẠO BUSINESS LAYER ======================
string connectionString = app.Configuration.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException("ConnectionString không tìm thấy!");

Configuration.Initialize(connectionString);
UserService.Initialize(connectionString);
ReportService.Initialize(connectionString);
DepartmentService.Initialize(connectionString);
CategoryService.Initialize(connectionString);
AssignmentService.Initialize(connectionString);
MapService.Initialize(connectionString);
// =====================================================================

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();