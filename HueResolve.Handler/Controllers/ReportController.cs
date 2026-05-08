using HueResolve.Business.Services;
using HueResolve.Models.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HueResolve.Handler.Controllers
{
    /// <summary>
    /// Controller điều phối nghiệp vụ xử lý phản ánh dành cho Cán bộ Đơn vị chức năng.
    /// Nguyên tắc phân quyền dữ liệu:
    ///   - Handler chỉ thấy phản ánh có AssignedDepartmentId == DepartmentId của mình.
    ///   - Handler chỉ được cập nhật trạng thái, không được phân công lại hoặc xóa.
    ///   - Bắt buộc có ResolutionNote khi chuyển sang HoanThanh hoặc TuChoi.
    /// </summary>
    [Authorize(Roles = "Handler")]
    public class ReportController : Controller
    {
        private const int PageSize = 10;

        /// <summary>
        /// Lấy DepartmentId của Handler từ Claim. Trả về -1 nếu không hợp lệ.
        /// </summary>
        private int GetMyDepartmentId()
        {
            var claim = User.FindFirstValue("DepartmentId");
            return int.TryParse(claim, out int id) ? id : -1;
        }

        /// <summary>
        /// GET: /Report/Index
        /// Hiển thị danh sách phản ánh được phân công cho đơn vị của Handler.
        /// Hỗ trợ lọc theo trạng thái, danh mục và tìm kiếm.
        /// Phản ánh NeedsReview được đánh dấu nổi bật để Handler chú ý.
        /// </summary>
        public async Task<IActionResult> Index(string? status = null, int? categoryId = null,
                                               string? search = null, int page = 1)
        {
            int departmentId = GetMyDepartmentId();
            if (departmentId == -1) return RedirectToAction("Login", "Account");

            if (page < 1) page = 1;

            // Lấy tất cả phản ánh, sau đó lọc theo đơn vị của Handler
            // Lưu ý: ReportService.GetPagedReportsAsync không có filter departmentId
            // nên lấy theo GetAllReportsAsync rồi lọc thủ công
            var allReports = await ReportService.GetAllReportsAsync(status, categoryId, search);
            var myReports = allReports
                .Where(r => r.AssignedDepartmentId == departmentId)
                .OrderByDescending(r => r.NeedsReview)   // NeedsReview lên đầu
                .ThenByDescending(r => r.CreatedAtUtc)
                .ToList();

            int totalCount = myReports.Count;
            var paged = myReports.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // Đếm theo từng trạng thái trong phạm vi đơn vị (để render tabs)
            var allMyReports = await ReportService.GetAllReportsAsync();
            var baseList = allMyReports.Where(r => r.AssignedDepartmentId == departmentId).ToList();

            ViewBag.CountAll = baseList.Count;
            ViewBag.CountTiepNhan = baseList.Count(r => r.Status == "TiepNhan");
            ViewBag.CountDangXuLy = baseList.Count(r => r.Status == "DangXuLy");
            ViewBag.CountHoanThanh = baseList.Count(r => r.Status == "HoanThanh");
            ViewBag.CountTuChoi = baseList.Count(r => r.Status == "TuChoi");

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();

            return View(paged);
        }

        /// <summary>
        /// GET: /Report/Details/{id}
        /// Xem chi tiết phản ánh, Timeline lịch sử và form cập nhật trạng thái.
        /// Kiểm tra an ninh: nếu phản ánh không thuộc đơn vị của Handler thì trả về 403 Forbidden.
        /// </summary>
        public async Task<IActionResult> Details(Guid id)
        {
            int departmentId = GetMyDepartmentId();
            if (departmentId == -1) return RedirectToAction("Login", "Account");

            if (id == Guid.Empty) return BadRequest();

            var report = await ReportService.GetReportByIdAsync(id);

            if (report == null) return NotFound();

            /// Bảo vệ dữ liệu: Handler không được xem phản ánh của đơn vị khác
            if (report.AssignedDepartmentId != departmentId)
                return Forbid();

            /// Load các dữ liệu liên quan
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            ViewBag.Assignment = await AssignmentService.GetLatestAssignmentAsync(id);
            ViewBag.Attachments = await ReportService.GetAttachmentsAsync(id);

            return View(report);
        }
        /// <summary>
        /// POST: Xử lý xóa ảnh minh chứng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(Guid attachmentId, Guid reportId)
        {
            int departmentId = GetMyDepartmentId();
            if (departmentId == -1) return RedirectToAction("Login", "Account");

            var report = await ReportService.GetReportByIdAsync(reportId);

            /// Kiểm tra bảo mật: Chỉ cho phép xóa khi phản ánh thuộc đơn vị và đang ở trạng thái DangXuLy
            if (report == null || report.AssignedDepartmentId != departmentId || report.Status != "DangXuLy")
                return Forbid();

            bool success = await ReportService.DeleteAttachmentAsync(attachmentId);

            if (success) TempData["Success"] = "Đã xóa ảnh minh chứng thành công.";
            else TempData["Error"] = "Không thể xóa ảnh. Vui lòng thử lại.";

            return RedirectToAction("Details", new { id = reportId });
        }

        /// <summary>
        /// POST: /Report/UpdateStatus
        /// Cập nhật trạng thái xử lý phản ánh.
        /// Ràng buộc nghiệp vụ:
        ///   - TiepNhan → DangXuLy: không bắt buộc ghi chú.
        ///   - DangXuLy → HoanThanh: BẮT BUỘC có ResolutionNote.
        ///   - DangXuLy → TuChoi: BẮT BUỘC có ResolutionNote (lý do từ chối).
        ///   - Không cho phép chuyển ngược trạng thái.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid reportId, string newStatus, string? resolutionNote)
        {
            int departmentId = GetMyDepartmentId();
            if (departmentId == -1) return RedirectToAction("Login", "Account");

            var report = await ReportService.GetReportByIdAsync(reportId);
            if (report == null || report.AssignedDepartmentId != departmentId)
                return Forbid();

            // Validate: bắt buộc ghi chú khi Hoàn thành hoặc Từ chối
            if ((newStatus == "HoanThanh" || newStatus == "TuChoi")
                && string.IsNullOrWhiteSpace(resolutionNote))
            {
                TempData["Error"] = "Vui lòng nhập ghi chú kết quả xử lý hoặc lý do từ chối.";
                return RedirectToAction("Details", new { id = reportId });
            }

            // Validate: chỉ cho phép các chuyển trạng thái hợp lệ
            bool isValidTransition =
                (report.Status == "TiepNhan" && newStatus == "DangXuLy") ||
                (report.Status == "DangXuLy" && (newStatus == "HoanThanh" || newStatus == "TuChoi"));

            if (!isValidTransition)
            {
                TempData["Error"] = "Chuyển trạng thái không hợp lệ.";
                return RedirectToAction("Details", new { id = reportId });
            }

            string handlerName = User.FindFirstValue(ClaimTypes.Name) ?? "Cán bộ xử lý";
            bool success = await ReportService.UpdateReportStatusAsync(reportId, newStatus, resolutionNote, handlerName);

            if (success)
            {
                string statusLabel = newStatus switch
                {
                    "DangXuLy" => "Đang xử lý",
                    "HoanThanh" => "Hoàn thành",
                    "TuChoi" => "Từ chối",
                    _ => newStatus
                };
                TempData["Success"] = $"Cập nhật trạng thái thành công: {statusLabel}.";
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng thử lại.";
            }

            return RedirectToAction("Details", new { id = reportId });
        }

        /// <summary>
        /// POST: Xử lý tải lên ảnh minh chứng (Lưu trực tiếp vào Database)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachment(Guid reportId, IFormFile file)
        {
            int departmentId = GetMyDepartmentId();
            if (departmentId == -1) return RedirectToAction("Login", "Account");

            var report = await ReportService.GetReportByIdAsync(reportId);
            if (report == null || report.AssignedDepartmentId != departmentId)
                return Forbid();

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file để tải lên.";
                return RedirectToAction("Details", new { id = reportId });
            }

            /// Giới hạn 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File vượt quá giới hạn 5MB.";
                return RedirectToAction("Details", new { id = reportId });
            }

            /// Chỉ chấp nhận ảnh và video
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "video/mp4", "video/quicktime" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                TempData["Error"] = "Định dạng file không được hỗ trợ.";
                return RedirectToAction("Details", new { id = reportId });
            }

            try
            {
                /// Đọc file thành mảng byte để lưu thẳng vào DB
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileData = memoryStream.ToArray();

                var attachment = new HueResolve.Models.Model.ReportAttachment
                {
                    Id = Guid.NewGuid(),
                    ReportId = reportId,
                    OriginalFileName = file.FileName,
                    StoredFileName = Guid.NewGuid().ToString(),
                    RelativePath = "DB_STORAGE", /// Không còn dùng đường dẫn vật lý
                    ContentType = file.ContentType,
                    CreatedAtUtc = DateTime.UtcNow,
                    FileData = fileData /// Lưu dữ liệu nhị phân
                };

                bool saved = await ReportService.SaveAttachmentAsync(reportId, attachment);
                TempData[saved ? "Success" : "Error"] = saved ? "Tải ảnh minh chứng thành công." : "Lưu thông tin file thất bại.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý file. Vui lòng thử lại.";
            }

            return RedirectToAction("Details", new { id = reportId });
        }
        /// <summary>
        /// Xử lý submit kết quả từ Cán bộ thực địa.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitResult(Guid reportId, string resultStatus, string resolutionNote)
        {
            var report = await ReportService.GetReportByIdAsync(reportId);

            /* Bảo mật: Phải đảm bảo phản ánh này đang được giao cho chính bộ phận của User hiện tại */
            if (report == null || report.AssignedDepartmentId != GetMyDepartmentId())
                return Unauthorized();

            bool success = await ReportService.SubmitReportResultAsync(reportId, resultStatus, resolutionNote);

            if (success)
            {
                TempData["Success"] = "Đã gửi báo cáo kết quả xử lý thành công.";
                return RedirectToAction(nameof(Index)); /* Trở lại danh sách nhiệm vụ của ReportController */
            }

            TempData["Error"] = "Lỗi khi lưu kết quả. Vui lòng thử lại.";
            return RedirectToAction(nameof(Details), new { id = reportId });
        }
    }
}