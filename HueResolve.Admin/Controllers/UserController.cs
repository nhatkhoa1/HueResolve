using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller điều hướng các chức năng quản lý tài khoản theo cấu trúc chuẩn.
    /// Hỗ trợ 4 vai trò: Admin (1), Officer (2), Customer (3), Handler (4).
    /// Handler là tài khoản trực thuộc đơn vị xử lý (Department), không thể khóa bởi Customer/Officer.
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        private const int PageSize = 10;

        /// <summary>
        /// Hiển thị danh sách tài khoản có phân trang, bộ lọc vai trò và tìm kiếm.
        /// </summary>
        public async Task<IActionResult> Index(int? roleId = null, string? search = null, int page = 1)
        {
            if (page < 1) page = 1;

            var (users, totalCount) = await UserService.GetPagedUsersAsync(page, PageSize, roleId, search);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentRoleId = roleId;
            ViewBag.CurrentSearch = search;

            return View(users);
        }

        /// <summary>
        /// Mở giao diện form đăng ký tài khoản.
        /// Dropdown Departments chỉ hiển thị các đơn vị đang hoạt động.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await DepartmentService.GetActiveDepartmentsAsync();
            return View();
        }

        /// <summary>
        /// Tiếp nhận và xử lý lưu trữ thông tin tài khoản.
        /// Mật khẩu được mã hóa MD5 trong UserService trước khi ghi xuống database.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống");
                ViewBag.Departments = await DepartmentService.GetActiveDepartmentsAsync();
                return View(user);
            }

            bool success = await UserService.CreateUserAsync(user, rawPassword);
            if (success)
            {
                TempData["Success"] = "Đã tạo tài khoản thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.Departments = await DepartmentService.GetActiveDepartmentsAsync();
            return View(user);
        }

        /// <summary>
        /// Xem chi tiết thông tin tài khoản người dùng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await UserService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        /// <summary>
        /// Mở giao diện form đổi mật khẩu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(Guid id)
        {
            var user = await UserService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Index");
            }

            ViewBag.UserId = id;
            ViewBag.Username = user.Username;
            return View();
        }

        /// <summary>
        /// Xử lý yêu cầu đổi mật khẩu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(Guid id, string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                ViewBag.UserId = id;
                var user = await UserService.GetUserByIdAsync(id);
                ViewBag.Username = user?.Username;
                return View();
            }

            bool success = await UserService.ChangePasswordAsync(id, currentPassword, newPassword);
            if (success)
            {
                TempData["Success"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Details", new { id });
            }

            ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác hoặc không thể cập nhật.");
            ViewBag.UserId = id;
            var targetUser = await UserService.GetUserByIdAsync(id);
            ViewBag.Username = targetUser?.Username;
            return View();
        }

        /// <summary>
        /// Xử lý yêu cầu thay đổi trạng thái hoạt động của tài khoản (Khóa/Mở khóa).
        /// Tài khoản Admin (RoleId = 1) được bảo vệ, không thể bị khóa bởi bất kỳ ai.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, bool isActive)
        {
            var targetUser = await UserService.GetUserByIdAsync(id);

            if (targetUser != null && targetUser.RoleId == 1)
            {
                TempData["Error"] = "Thao tác bị từ chối! Không được phép khóa tài khoản Quản trị viên hệ thống.";
                return RedirectToAction("Index");
            }

            bool success = await UserService.SetUserActiveAsync(id, isActive);
            if (success)
            {
                TempData["Success"] = isActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}