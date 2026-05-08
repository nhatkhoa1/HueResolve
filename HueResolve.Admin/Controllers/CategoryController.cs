using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý danh mục phân loại sự cố, phục vụ công tác hiển thị và dữ liệu train AI.
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        private const int PageSize = 10;

        /// <summary>Hiển thị danh sách các lĩnh vực sự cố (có phân trang).</summary>
        public async Task<IActionResult> Index(string? search = null, int page = 1)
        {
            if (page < 1) page = 1;
            var (categories, totalCount) = await CategoryService.GetPagedCategoriesAsync(page, PageSize, search);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentSearch = search;

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                bool success = await CategoryService.CreateCategoryAsync(category);
                if (success)
                {
                    TempData["Success"] = "Thêm danh mục mới thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await CategoryService.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                bool success = await CategoryService.UpdateCategoryAsync(category);
                if (success)
                {
                    TempData["Success"] = "Cập nhật danh mục thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        /// <summary>
        /// POST: Xóa một lĩnh vực sự cố.
        /// Chặn bắt lỗi nếu danh mục này đang chứa các phản ánh (Foreign Key Constraint).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool success = await CategoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["Success"] = "Đã xóa danh mục thành công.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy danh mục để xóa.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì đang có các phản ánh liên kết với nó.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}