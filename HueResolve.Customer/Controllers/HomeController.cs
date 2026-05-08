using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;

namespace HueResolve.Customer.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Trang chủ: Xem danh sách công khai và Heatmap.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var reports = await ReportService.GetAllReportsAsync();
            /// Chỉ lấy các phản ánh đã duyệt (không lấy hàng rác/nhạy cảm)
            var publicReports = reports.Where(r => r.Status != "TuChoi").Take(10).ToList();

            return View(publicReports);
        }

        /// <summary>
        /// Xử lý tra cứu nhanh qua mã TrackingCode.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Track(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return RedirectToAction("Index");

            var report = await ReportService.GetByTrackingCodeAsync(code.Trim());
            if (report == null)
            {
                TempData["SearchError"] = "Không tìm thấy phản ánh với mã này.";
                return RedirectToAction("Index");
            }

            /// Gọi Service để lấy danh sách ảnh đính kèm của phản ánh này
            var attachments = await ReportService.GetAttachmentsAsync(report.Id);

            /// Sử dụng ViewBag để truyền danh sách ảnh sang View
            ViewBag.Attachments = attachments;

            return View(report);
        }
        /// <summary>
        /// API cung cấp dữ liệu bản đồ cho Landing Page (Chế độ Public).
        /// Đã lọc bỏ dữ liệu nhạy cảm và các phản ánh không hợp lệ.
        /// </summary>
        [HttpGet]
        /// <summary>
        /// API cung cấp dữ liệu bản đồ cho người dân.
        /// Hiển thị các điểm: Mới tiếp nhận, Đang xử lý, và Đã hoàn thành.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPublicMapData()
        {
            var rawData = await MapService.GetMapDataAsync();

            /// Cho phép DangXuLy xuất hiện trên bản đồ công khai
            var publicData = rawData
                .Where(r => r.Status == "TiepNhan" || r.Status == "DangXuLy" || r.Status == "HoanThanh")
                .Select(r => new
                {
                    r.TrackingCode,
                    r.Title,
                    r.Latitude,
                    r.Longitude,
                    r.Status,
                    r.AddressText
                });

            return Json(publicData);
        }
    }
}