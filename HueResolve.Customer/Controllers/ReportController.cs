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
        public async Task<IActionResult> Create(Report model, IFormFile? attachmentFile)
        {
            try
            {
                /// 1. Bổ sung các thông tin ẩn cho Report
                /// Lấy CustomerId từ phiên đăng nhập hiện tại
                string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                model.CustomerId = Guid.Parse(userIdString);

                model.Id = Guid.NewGuid();
                model.Status = "TiepNhan";
                model.CreatedAtUtc = DateTime.UtcNow;
                model.NeedsReview = true; /// Mặc định cần duyệt nếu AI chưa chạy (Sẽ tích hợp AI sau)

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
                    if (attachmentFile != null && attachmentFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFile.CopyToAsync(memoryStream);
                        byte[] fileData = memoryStream.ToArray();

                        var attachment = new ReportAttachment
                        {
                            Id = Guid.NewGuid(),
                            ReportId = model.Id,
                            OriginalFileName = attachmentFile.FileName,
                            StoredFileName = Guid.NewGuid().ToString(),
                            RelativePath = "DB_STORAGE",
                            ContentType = attachmentFile.ContentType,
                            CreatedAtUtc = DateTime.UtcNow,
                            FileData = fileData
                        };

                        await ReportService.SaveAttachmentAsync(model.Id, attachment);
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
    }
}