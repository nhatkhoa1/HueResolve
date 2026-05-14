using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;
using System.Security.Claims;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller điều phối quy trình xử lý phản ánh hiện trường.
    /// Đã tích hợp đầy đủ các Action cập nhật trạng thái đồng bộ với Service.
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        private const int PageSize = 10;

        /// <summary>
        /// Hiển thị danh sách phản ánh công việc.
        /// </summary>
        public async Task<IActionResult> Index(string? status = null, int? categoryId = null, string? search = null, int page = 1)
        {
            if (page < 1) page = 1;
            status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            var (reports, totalCount) = await ReportService.GetPagedReportsAsync(page, PageSize, status, categoryId, search);
            var stats = await ReportService.GetDashboardStatsAsync();

            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            ViewBag.CountAll = stats.TotalReports;
            ViewBag.CountTiepNhan = stats.Pending;
            ViewBag.CountDangXuLy = stats.Processing;
            ViewBag.CountChoDuyetKq = stats.PendingApproval;
            ViewBag.CountHoanThanh = stats.Completed;
            ViewBag.CountTuChoi = stats.Rejected;

            return View(reports);
        }

        /// <summary>
        /// Hiển thị chi tiết và Timeline xử lý phản ánh.
        /// </summary>
        /// <summary>
        /// Hiển thị chi tiết phản ánh cho Admin/Operator.
        /// </summary>
        public async Task<IActionResult> Details(Guid id)
        {
            var report = await ReportService.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            ViewBag.Timeline = await ReportService.GetHistoryByReportIdAsync(id);
            ViewBag.Attachments = await ReportService.GetAttachmentsAsync(id); /// Hoặc GetAttachmentsByReportIdAsync tùy tên bạn đặt
            ViewBag.Departments = await DepartmentService.GetActiveDepartmentsAsync();
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            ViewBag.LatestAssignment = await AssignmentService.GetLatestAssignmentAsync(id);

            /// ĐÃ XÓA ViewBag.ImageHost Ở ĐÂY VÌ KHÔNG CẦN THIẾT NỮA

            return View(report);
        }

        /// <summary>
        /// Action xử lý cập nhật trạng thái chung (VD: Hoàn thành).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status, string? note)
        {
            if (id == Guid.Empty) return BadRequest();

            string userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            bool success = await ReportService.UpdateReportStatusAsync(id, status, note, userName);

            if (success)
            {
                TempData["Success"] = $"Đã cập nhật trạng thái thành: {status}";
            }
            else
            {
                TempData["Error"] = "Cập nhật trạng thái thất bại.";
            }

            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Mở form phân công đơn vị xử lý.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AssignDepartment(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var report = await ReportService.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            ViewBag.Departments = await DepartmentService.GetActiveDepartmentsAsync();
            return View(report);
        }

        /// <summary>
        /// Xử lý phân công đơn vị.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDepartment(Guid id, int departmentId, string note)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

            if (!Guid.TryParse(userIdString, out Guid assignedByUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool success = await AssignmentService.AssignReportToDepartmentAsync(id, departmentId, assignedByUserId, note, userName);

            if (success)
            {
                /// Gán thông báo thành công vào TempData
                TempData["Success"] = "Hệ thống đã điều phối đơn vị xử lý thành công!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Phân công thất bại. Vui lòng kiểm tra lại dữ liệu.";
            return RedirectToAction("AssignDepartment", new { id = id });
        }

        /// <summary>
        /// Lưu phản hồi từ Cán bộ IOC.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeedback(Guid reportId, string adminFeedback)
        {
            bool success = await ReportService.SaveAdminFeedbackAsync(reportId, adminFeedback);
            if (success)
            {
                TempData["Success"] = "Đã gửi phản hồi thành công.";
            }
            else
            {
                TempData["Error"] = "Gửi phản hồi thất bại.";
            }
            return RedirectToAction("Details", new { id = reportId });
        }

        /// <summary>
        /// Xử lý từ chối phản ánh.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            string userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

            bool success = await ReportService.RejectReportAsync(id, reason, userName);
            if (success)
            {
                TempData["Success"] = "Đã từ chối phản ánh thành công.";
            }
            else
            {
                TempData["Error"] = "Lỗi cập nhật trạng thái từ chối.";
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xử lý duyệt kết quả từ Cán bộ thực địa.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveResult(Guid id, bool isApproved, string adminNote)
        {
            if (id == Guid.Empty) return BadRequest();

            var report = await ReportService.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            string userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            
            string newStatus = "DangXuLy";
            if (isApproved)
            {
                if (!string.IsNullOrEmpty(report.ResolutionNote) && report.ResolutionNote.Contains("[Đề xuất Từ chối]"))
                {
                    newStatus = "TuChoi";
                }
                else
                {
                    newStatus = "HoanThanh";
                }
            }

            string finalNote = isApproved ? $"[Duyệt kết quả] {adminNote}" : $"[Yêu cầu xử lý lại] {adminNote}";

            bool success = await ReportService.UpdateReportStatusAsync(id, newStatus, finalNote, userName);

            if (success)
            {
                // Lưu ghi chú của Admin vào AdminFeedback để Handler/Customer có thể đọc được
                await ReportService.SaveAdminFeedbackAsync(id, adminNote);

                if (isApproved)
                    TempData["Success"] = "Đã duyệt và gửi kết quả cho công dân thành công.";
                else
                    TempData["Success"] = "Đã trả lại phản ánh cho Đơn vị để xử lý lại.";
            }
            else
            {
                TempData["Error"] = "Lỗi xử lý duyệt kết quả.";
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}