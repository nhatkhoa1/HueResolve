using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HueResolve.Business.Services;

namespace HueResolve.Handler.Controllers
{
    /// <summary>
    /// Controller trung tâm hiển thị bảng điều khiển thống kê dành riêng cho Cán bộ Đơn vị.
    /// Dữ liệu được lọc tự động theo DepartmentId của người dùng đang đăng nhập.
    /// </summary>
    [Authorize(Roles = "Handler")]
    public class DashboardController : Controller
    {
        /// <summary>
        /// Xử lý hiển thị trang chủ Dashboard.
        /// Tính toán các chỉ số: Tiếp nhận, Đang xử lý, Quá hạn và danh sách phản ánh mới nhất.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            /// Lấy ID đơn vị từ Claims Identity
            var deptIdClaim = User.FindFirstValue("DepartmentId");
            if (!int.TryParse(deptIdClaim, out int departmentId))
            {
                return RedirectToAction("Logout", "Account");
            }

            var allReports = await ReportService.GetAllReportsAsync();

            /// Lọc danh sách phản ánh thuộc thẩm quyền của đơn vị này
            var myReports = allReports.Where(r => r.AssignedDepartmentId == departmentId).ToList();

            /// Thống kê cơ bản
            ViewBag.TotalAssigned = myReports.Count;
            ViewBag.CountTiepNhan = myReports.Count(r => r.Status == "TiepNhan");
            ViewBag.CountDangXuLy = myReports.Count(r => r.Status == "DangXuLy");
            ViewBag.CountHoanThanh = myReports.Count(r => r.Status == "HoanThanh");

            /// Logic tính toán phản ánh "Quá hạn" (SLA mặc định 5 ngày theo đặc tả)
            DateTime deadlineLimit = DateTime.UtcNow.AddDays(-5);
            ViewBag.CountQuaHan = myReports.Count(r =>
                r.Status != "HoanThanh" &&
                r.Status != "TuChoi" &&
                r.CreatedAtUtc < deadlineLimit);

            ViewBag.DepartmentName = User.FindFirstValue("DepartmentName");

            /// Lấy 6 phản ánh mới nhất để hiển thị bảng tin
            var recentReports = myReports
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(6);

            return View(recentReports);
        }
        /// <summary>
        /// API lấy danh sách thông báo (nhiệm vụ mới) cho Đơn vị xử lý.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var deptIdClaim = User.FindFirstValue("DepartmentId");
            if (!int.TryParse(deptIdClaim, out int departmentId))
            {
                return Json(new { count = 0, items = new object[0] });
            }

            var allReports = await ReportService.GetAllReportsAsync();
            var newAssignments = allReports
                .Where(r => r.AssignedDepartmentId == departmentId && r.Status == "TiepNhan")
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToList();

            var items = newAssignments.Take(5).Select(r => new
            {
                id = r.Id,
                trackingCode = r.TrackingCode,
                title = "Phân công xử lý mới",
                note = r.Title.Length > 50 ? r.Title.Substring(0, 47) + "..." : r.Title,
                timeAgo = GetTimeAgo(r.CreatedAtUtc)
            });

            return Json(new { count = newAssignments.Count, items });
        }

        private string GetTimeAgo(DateTime past)
        {
            var span = DateTime.UtcNow - past;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            return $"{(int)span.TotalDays} ngày trước";
        }
    }
}