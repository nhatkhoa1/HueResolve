using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;

namespace HueResolve.Customer.Controllers
{
    /// <summary>
    /// Controller xử lý xác thực tài khoản sử dụng mã hóa MD5 đồng bộ với Admin.
    /// </summary>
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            var user = await UserService.AuthenticateAsync(username, password);  // ← Truyền raw password

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View();
            }

            if (user.RoleId != 3)
            {
                ViewBag.Error = "Tài khoản này không có quyền truy cập Cổng Công Dân.";
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Role, "Citizen"),
        new Claim("Username", user.Username)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string username, string password, string phoneNumber, string addressText)
        {
            var existingUser = await UserService.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã tồn tại.";
                return View();
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Username = username,
                RoleId = 3, /// Role mặc định cho người dân
                CreatedAtUtc = DateTime.UtcNow,
                PhoneNumber = phoneNumber,
                AddressText = addressText
            };

            /// Bước quan trọng: Sử dụng MD5 để băm mật khẩu trước khi lưu
            /// Lưu ý: Đảm bảo hàm CreateUserAsync trong UserService của bạn lưu thẳng giá trị password truyền vào 
            /// vào cột PasswordHash nếu bạn đã hash ở đây.
            string md5Password = GetMd5Hash(password);
            bool isCreated = await UserService.CreateUserAsync(newUser, md5Password);

            if (isCreated)
            {
                TempData["Success"] = "Đăng ký thành công! Mời bạn đăng nhập.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Lỗi hệ thống, vui lòng thử lại sau.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Xem thông tin cá nhân của người dùng hiện tại.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return RedirectToAction("Login");

            var user = await UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        /// <summary>
        /// Mở giao diện form chỉnh sửa thông tin cá nhân.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return RedirectToAction("Login");

            var user = await UserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân.
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string fullName, string? phoneNumber, string? addressText)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ViewBag.Error = "Họ tên không được để trống.";
                var user = await UserService.GetUserByIdAsync(userId);
                return View(user);
            }

            bool success = await UserService.UpdateUserInfoAsync(userId, fullName, phoneNumber, addressText);
            if (success)
            {
                TempData["Success"] = "Cập nhật thông tin thành công.";
                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Cập nhật thất bại. Vui lòng thử lại.";
            var currentUser = await UserService.GetUserByIdAsync(userId);
            return View(currentUser);
        }

        /// <summary>
        /// Mở giao diện form đổi mật khẩu.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return RedirectToAction("Login");

            ViewBag.UserId = userId;
            return View();
        }

        /// <summary>
        /// Xử lý yêu cầu đổi mật khẩu.
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return RedirectToAction("Login");

            ViewBag.UserId = userId;

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return View();
            }

            bool success = await UserService.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (success)
            {
                TempData["Success"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Mật khẩu hiện tại không chính xác hoặc không thể cập nhật.";
            return View();
        }

        /// <summary>
        /// Hàm Helper mã hóa chuỗi thành MD5 (Viết gọn giống Admin)
        /// </summary>
        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}