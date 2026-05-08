using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HueResolve.Business.Services;

namespace HueResolve.Handler.Controllers
{
    /// <summary>
    /// Controller xử lý xác thực cho Cán bộ Đơn vị chức năng (Handler).
    /// Handler được định nghĩa bằng RoleId = 4, luôn có DepartmentId hợp lệ.
    /// Các vai trò khác (Admin=1, Officer=2, Customer=3) bị từ chối truy cập hệ thống này.
    /// Sau khi xác thực thành công, DepartmentId và DepartmentName được ghi vào Claims
    /// để các Controller khác lọc dữ liệu đúng phạm vi đơn vị.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// GET: /Account/Login
        /// Hiển thị trang đăng nhập. Nếu đã xác thực hợp lệ thì chuyển hướng về Dashboard.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// Xử lý đăng nhập cho Handler.
        /// Quy tắc phân quyền:
        ///   - Sai thông tin đăng nhập → báo lỗi chung.
        ///   - RoleId != 4 → không phải Handler, từ chối với thông báo rõ ràng.
        ///   - RoleId == 4 nhưng DepartmentId == null → dữ liệu không hợp lệ, từ chối.
        ///   - RoleId == 4 và DepartmentId != null → hợp lệ, tạo Cookie và chuyển Dashboard.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            ViewBag.Username = username;

            var user = await UserService.AuthenticateAsync(username.Trim(), password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            if (user.RoleId != 4)
            {
                ViewBag.Error = "Tài khoản không có quyền truy cập hệ thống Cán bộ Xử lý.";
                return View();
            }

            if (!user.DepartmentId.HasValue)
            {
                ViewBag.Error = "Tài khoản chưa được gắn với đơn vị xử lý. Vui lòng liên hệ Quản trị viên.";
                return View();
            }

            var department = await DepartmentService.GetDepartmentByIdAsync(user.DepartmentId.Value);
            string departmentName = department?.Name ?? "Đơn vị chức năng";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, "Handler"),
                new Claim("Username", user.Username),
                new Claim("DepartmentId", user.DepartmentId.Value.ToString()),
                new Claim("DepartmentName", departmentName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// POST: /Account/Logout
        /// Đăng xuất, xóa Cookie phiên làm việc và chuyển hướng về trang Login.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}