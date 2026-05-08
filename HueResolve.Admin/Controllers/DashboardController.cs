using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller hiển thị trang Tổng quan (Dashboard) dành cho Admin và Officer.
    /// Lấy số liệu thống kê và danh sách phản ánh mới nhất từ ReportService.
    /// Yêu cầu đăng nhập — [Authorize] chặn truy cập nặc danh, tự động redirect về /Account/Login.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        /// <summary>
        /// GET: /Dashboard
        /// Hiển thị trang tổng quan với 4 thẻ thống kê và danh sách 10 phản ánh mới nhất.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var stats = await ReportService.GetDashboardStatsAsync();
            var recentReports = await ReportService.GetRecentReportsAsync(10);
            ViewBag.Total = stats.TotalReports;
            ViewBag.DangXuLy = stats.Processing;
            ViewBag.HoanThanh = stats.Completed;
            ViewBag.TuChoi = stats.Rejected;
            ViewBag.ChartGiaoThong = stats.GiaoThong;
            ViewBag.ChartMoiTruong = stats.MoiTruong;
            ViewBag.ChartHaTang = stats.HaTang;
            ViewBag.ChartAnNinh = stats.AnNinh;
            ViewBag.ChartKhac = stats.Khac;

            return View(recentReports);
        }
    }
}