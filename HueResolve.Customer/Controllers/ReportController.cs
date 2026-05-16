using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;

namespace HueResolve.Customer.Controllers
{
    /// <summary>
    /// Quản lý các thao tác Gửi phản ánh và Xem lịch sử cá nhân của Người dân.
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        /// <summary>
        /// GET: Hiển thị Form gửi phản ánh mới.
        /// Nạp danh sách Category để hiển thị lên Dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            return View();
        }

        /// <summary>
        /// POST: Xử lý dữ liệu người dân gửi lên (Lưu phản ánh + Lưu ảnh Base64).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Report model, List<IFormFile>? attachmentFiles)
        {
            try
            {
                if (attachmentFiles != null && attachmentFiles.Count > 0)
                {
                    int imgCount = 0, vidCount = 0;
                    foreach (var f in attachmentFiles)
                    {
                        if (f.ContentType.StartsWith("image/")) imgCount++;
                        else if (f.ContentType.StartsWith("video/")) vidCount++;
                        else
                        {
                            ViewBag.Error = "Định dạng file không được hỗ trợ. Chỉ cho phép ảnh và video.";
                            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
                            return View(model);
                        }
                    }
                    if (imgCount > 3 || vidCount > 1)
                    {
                        ViewBag.Error = "Chỉ được phép tải lên tối đa 3 ảnh và 1 video.";
                        ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
                        return View(model);
                    }
                }

                /// 1. Bổ sung các thông tin ẩn cho Report
                /// Lấy CustomerId từ phiên đăng nhập hiện tại
                string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                model.CustomerId = Guid.Parse(userIdString);

                var currentUser = await UserService.GetUserByIdAsync(model.CustomerId.Value);
                if (currentUser != null)
                {
                    model.ReporterName = currentUser.FullName;
                    model.ReporterPhone = currentUser.PhoneNumber ?? string.Empty;
                }

                model.Id = Guid.NewGuid();
                model.Status = "TiepNhan";
                model.CreatedAtUtc = DateTime.UtcNow;

                /// Tích hợp AI phân loại tự động nếu người dân không chọn Lĩnh vực
                if (!model.CategoryId.HasValue || model.CategoryId == 0)
                {
                    var predictedCategory = await AIService.PredictCategoryAsync(model.Title, model.Description);
                    if (predictedCategory != null)
                    {
                        model.CategoryId = predictedCategory.Id;
                        model.ClassificationState = "AI_Auto_Classified";
                        model.AiConfidence = 0.85; /// Mock độ tin cậy
                    }
                }

                model.NeedsReview = true; /// Mặc định cần duyệt lại kết quả AI hoặc phân loại thủ công

                /// Tạo mã tra cứu ngẫu nhiên (Ví dụ: HUE-20260507-1234)
                string datePart = DateTime.Now.ToString("yyyyMMdd");
                string randomPart = new Random().Next(1000, 9999).ToString();
                model.TrackingCode = $"HUE-{datePart}-{randomPart}";

                /// 2. Lưu Report vào Database (Giả sử bạn đã có hàm InsertAsync trong ReportService)
                /// Nếu chưa có, bạn chỉ cần gọi _reportRepository.InsertAsync(model) ở tầng Business
                bool isReportSaved = await ReportService.CreateReportAsync(model);

                if (isReportSaved)
                {
                    /// 3. Xử lý lưu File đính kèm dưới dạng Mảng Byte (Base64)
                    if (attachmentFiles != null && attachmentFiles.Count > 0)
                    {
                        foreach (var f in attachmentFiles)
                        {
                            using var memoryStream = new MemoryStream();
                            await f.CopyToAsync(memoryStream);
                            byte[] fileData = memoryStream.ToArray();

                            var attachment = new ReportAttachment
                            {
                                Id = Guid.NewGuid(),
                                ReportId = model.Id,
                                OriginalFileName = f.FileName,
                                StoredFileName = Guid.NewGuid().ToString(),
                                RelativePath = "DB_STORAGE",
                                ContentType = f.ContentType,
                                CreatedAtUtc = DateTime.UtcNow,
                                FileData = fileData,
                                AttachmentType = "Citizen"
                            };

                            await ReportService.SaveAttachmentAsync(model.Id, attachment);
                        }
                    }

                    TempData["Success"] = $"Gửi phản ánh thành công! Mã tra cứu của bạn là {model.TrackingCode}";
                    return RedirectToAction("Track", "Home", new { code = model.TrackingCode });
                }

                ViewBag.Error = "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
            }

            /// Nếu có lỗi, load lại danh mục và trả về View
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            return View(model);
        }
        /// <summary>
        /// GET: Lấy danh sách toàn bộ phản ánh do người dân (Customer hiện tại) đã gửi.
        /// </summary>
        [HttpGet]
        /// <summary>
        /// GET: Lấy danh sách phản ánh do người dân đã gửi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!Guid.TryParse(userIdString, out Guid customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            /// Lấy danh sách tối ưu từ Database thông qua Service
            var myReports = await ReportService.GetReportsByCustomerIdAsync(customerId);

            return View(myReports);
        }

        /// <summary>
        /// POST: Xóa phản ánh của người dân (chỉ xóa được khi chưa giao cho đơn vị xử lý).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!Guid.TryParse(userIdString, out Guid customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var report = await ReportService.GetReportByIdAsync(id);
            if (report == null || report.CustomerId != customerId)
            {
                TempData["Error"] = "Không tìm thấy phản ánh hoặc bạn không có quyền xóa.";
                return RedirectToAction("MyReports");
            }

            if (report.AssignedDepartmentId.HasValue)
            {
                TempData["Error"] = "Không thể xóa phản ánh đã được giao cho đơn vị xử lý.";
                return RedirectToAction("MyReports");
            }

            bool success = await ReportService.DeleteReportAsync(id);
            if (success)
            {
                TempData["Success"] = "Đã xóa phản ánh thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa phản ánh.";
            }

            return RedirectToAction("MyReports");
        }
    }
}