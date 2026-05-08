using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller điều phối giao diện và cung cấp API dữ liệu GIS cho bản đồ Điểm nóng.
    /// </summary>
    [Authorize]
    public class MapController : Controller
    {
        /// <summary>
        /// GET: /Map/Index
        /// Render giao diện khung bản đồ số và bộ lọc.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await CategoryService.GetAllCategoriesAsync();
            return View();
        }

        /// <summary>
        /// API trả về dữ liệu định dạng JSON để Javascript render các Marker lên bản đồ.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMapData(int? categoryId)
        {
            var reports = await MapService.GetMapDataAsync(categoryId);
            return Json(reports);
        }
    }
}