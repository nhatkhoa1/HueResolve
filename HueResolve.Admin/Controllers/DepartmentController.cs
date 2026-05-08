using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HueResolve.Business.Services;
using HueResolve.Models.Model;

namespace HueResolve.Admin.Controllers
{
    /// <summary>
    /// Controller chịu trách nhiệm điều phối các luồng nghiệp vụ liên quan đến Đơn vị xử lý (Department).
    /// Hỗ trợ các tính năng CRUD (Thêm, Sửa, Xóa, Liệt kê) và kích hoạt/vô hiệu hóa đơn vị.
    /// Yêu cầu người dùng phải đăng nhập hợp lệ ([Authorize]) để truy cập.
    /// </summary>
    [Authorize]
    public class DepartmentController : Controller
    {
        /// <summary>
        /// Cấu hình số lượng bản ghi tối đa hiển thị trên mỗi trang danh sách.
        /// </summary>
        private const int PageSize = 10;

        /// <summary>
        /// [GET] Hiển thị màn hình danh sách Đơn vị xử lý với cơ chế phân trang và bộ lọc tìm kiếm.
        /// </summary>
        /// <param name="search">Từ khóa tìm kiếm theo tên, loại hình hoặc thông tin liên hệ. Truyền <c>null</c> để lấy toàn bộ.</param>
        /// <param name="page">Số thứ tự trang hiện tại. Mặc định là 1.</param>
        /// <returns>View <c>Index.cshtml</c> chứa tập dữ liệu <c>IEnumerable&lt;Department&gt;</c>.</returns>
        public async Task<IActionResult> Index(string? search = null, int page = 1)
        {
            if (page < 1) page = 1;
            var (departments, totalCount) = await DepartmentService.GetPagedDepartmentsAsync(page, PageSize, search);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ViewBag.CurrentSearch = search;

            return View(departments);
        }

        /// <summary>
        /// [GET] Trả về giao diện Form thêm mới Đơn vị xử lý.
        /// </summary>
        /// <returns>View <c>Create.cshtml</c>.</returns>
        [HttpGet]
        public IActionResult Create() => View();

        /// <summary>
        /// [POST] Xử lý dữ liệu từ Form để lưu một Đơn vị mới vào cơ sở dữ liệu.
        /// Bao gồm cơ chế chống giả mạo request (CSRF) thông qua <c>[ValidateAntiForgeryToken]</c>.
        /// </summary>
        /// <param name="department">Đối tượng <see cref="Department"/> chứa dữ liệu post lên từ trình duyệt.</param>
        /// <returns>
        /// Nếu thành công: Chuyển hướng (Redirect) về trang Danh sách (Index) kèm thông báo.
        /// Nếu dữ liệu không hợp lệ: Render lại form tạo mới kèm thông báo lỗi Validate.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                bool success = await DepartmentService.CreateDepartmentAsync(department);
                if (success)
                {
                    TempData["Success"] = "Thêm đơn vị mới thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(department);
        }

        /// <summary>
        /// [GET] Lấy thông tin hiện tại của Đơn vị từ DB và hiển thị lên Form chỉnh sửa.
        /// </summary>
        /// <param name="id">Mã định danh của Đơn vị cần sửa.</param>
        /// <returns>View <c>Edit.cshtml</c> nếu tìm thấy đơn vị; ngược lại trả về mã lỗi 404 (NotFound).</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dept = await DepartmentService.GetDepartmentByIdAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        /// <summary>
        /// [POST] Xử lý cập nhật thông tin đã chỉnh sửa của Đơn vị vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="department">Đối tượng <see cref="Department"/> mang dữ liệu mới do người dùng Submit.</param>
        /// <returns>Chuyển hướng về trang Index nếu thành công, hoặc trả lại Form nếu dữ liệu nhập sai.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                bool success = await DepartmentService.UpdateDepartmentAsync(department);
                if (success)
                {
                    TempData["Success"] = "Cập nhật thông tin đơn vị thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(department);
        }

        /// <summary>
        /// [GET] /Department/Delete/{id}
        /// Truy xuất và hiển thị trang xác nhận an toàn trước khi xóa vật lý một Đơn vị.
        /// Ngăn chặn việc lỡ tay bấm nhầm xóa mất dữ liệu quan trọng.
        /// </summary>
        /// <param name="id">Khóa chính của Đơn vị cần xem xét xóa.</param>
        /// <returns>View <c>Delete.cshtml</c> nếu tìm thấy đơn vị; nếu không trả về HTTP 404.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await DepartmentService.GetDepartmentByIdAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        /// <summary>
        /// [POST] /Department/Delete
        /// ActionName="Delete" chỉ định hàm này sẽ xử lý POST request từ form của route Delete.
        /// Thực hiện lệnh xóa vật lý (Hard Delete) khỏi cơ sở dữ liệu.
        /// Có khối Try-Catch để bắt ngoại lệ (Foreign Key Constraints) nếu đơn vị đang bị ràng buộc bởi các phản ánh.
        /// </summary>
        /// <param name="id">ID của đơn vị cần tiêu hủy.</param>
        /// <returns>Chuyển hướng về trang Index kèm thông báo kết quả Xóa thành công hoặc báo lỗi vi phạm liên kết.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                bool success = await DepartmentService.DeleteDepartmentAsync(id);
                if (success)
                {
                    TempData["Success"] = "Đã xóa đơn vị thành công.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy đơn vị để xóa.";
                }
            }
            catch (Exception)
            {
                /// Bắt ngoại lệ từ SqlException khi vi phạm ràng buộc khóa ngoại (FK)
                TempData["Error"] = "Không thể xóa đơn vị này vì đang có dữ liệu liên kết (phản ánh hoặc tài khoản).";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// [POST] Xử lý nghiệp vụ thay đổi trạng thái kích hoạt (Khóa / Mở khóa) của một Đơn vị.
        /// Tính năng này được tích hợp gọi thông qua Modal trực tiếp từ màn hình Index.
        /// </summary>
        /// <param name="id">ID của đơn vị bị thay đổi.</param>
        /// <param name="isActive">Cờ trạng thái mới: <c>true</c> (Mở khóa) / <c>false</c> (Khóa).</param>
        /// <returns>Chuyển hướng (Reload) lại màn hình Index kèm theo thông báo trạng thái cập nhật.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            bool success = await DepartmentService.SetDepartmentActiveAsync(id, isActive);
            if (success)
            {
                TempData["Success"] = isActive ? "Đã mở khóa đơn vị." : "Đã khóa đơn vị xử lý.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}