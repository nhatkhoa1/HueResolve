using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Lớp dịch vụ tĩnh (Static Service) chịu trách nhiệm quản lý toàn bộ các quy tắc nghiệp vụ (Business Logic) 
    /// liên quan đến thực thể Danh mục / Lĩnh vực sự cố (Category).
    /// Hoạt động như một cầu nối trung gian giữa tầng Giao diện (UI) và tầng Truy xuất dữ liệu (Data Access).
    /// </summary>
    public static class CategoryService
    {
        private static ICategoryRepository _categoryRepository = default!;

        /// <summary>
        /// Khởi tạo dịch vụ danh mục bằng cách tiêm (inject) chuỗi kết nối cơ sở dữ liệu.
        /// Phương thức này CẦN được gọi một lần duy nhất lúc khởi động ứng dụng (ví dụ: trong Program.cs).
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối hợp lệ tới SQL Server.</param>
        public static void Initialize(string connectionString)
        {
            _categoryRepository = new CategoryRepository(connectionString);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách các danh mục (lĩnh vực sự cố) hiện có trong hệ thống.
        /// Thường được sử dụng để đổ dữ liệu vào các Dropdown List (Thẻ Select) trên giao diện Thêm/Sửa phản ánh.
        /// </summary>
        /// <returns>Một tập hợp <see cref="IEnumerable{Category}"/> chứa tất cả danh mục.</returns>
        public static async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        /// <summary>
        /// Truy xuất thông tin chi tiết của một danh mục cụ thể dựa trên khóa chính (Id).
        /// </summary>
        /// <param name="id">Định danh (Mã số) của danh mục cần tìm.</param>
        /// <returns>Đối tượng <see cref="Category"/> nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
        public static async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Lấy danh sách danh mục có phân trang, hỗ trợ tìm kiếm theo từ khóa.
        /// Được thiết kế tối ưu để hiển thị trên màn hình Quản lý Danh mục của Admin.
        /// </summary>
        /// <param name="page">Số thứ tự trang cần lấy (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số lượng danh mục tối đa trên mỗi trang.</param>
        /// <param name="search">Từ khóa tìm kiếm linh hoạt (tìm theo Tên hoặc Mã danh mục). Có thể truyền null nếu không tìm kiếm.</param>
        /// <returns>Một Tuple chứa: Danh sách dữ liệu của trang hiện tại (<c>Data</c>) và Tổng số bản ghi tìm được (<c>TotalCount</c>).</returns>
        public static async Task<(IEnumerable<Category> Data, int TotalCount)> GetPagedCategoriesAsync(int page, int pageSize, string? search)
        {
            return await _categoryRepository.GetPagedAsync(page, pageSize, search);
        }

        /// <summary>
        /// Thực thi nghiệp vụ thêm mới một Danh mục / Lĩnh vực sự cố vào hệ thống.
        /// </summary>
        /// <param name="category">Đối tượng <see cref="Category"/> chứa các thông tin cần thêm mới.</param>
        /// <returns><c>true</c> nếu lưu thành công vào CSDL; ngược lại trả về <c>false</c>.</returns>
        public static async Task<bool> CreateCategoryAsync(Category category)
        {
            int result = await _categoryRepository.InsertAsync(category);
            return result > 0;
        }

        /// <summary>
        /// Thực thi nghiệp vụ cập nhật thông tin của một Danh mục hiện có (ví dụ: Đổi tên, sửa mô tả AI).
        /// </summary>
        /// <param name="category">Đối tượng <see cref="Category"/> chứa thông tin đã được chỉnh sửa.</param>
        /// <returns><c>true</c> nếu cập nhật thành công; ngược lại trả về <c>false</c>.</returns>
        public static async Task<bool> UpdateCategoryAsync(Category category)
        {
            int result = await _categoryRepository.UpdateAsync(category);
            return result > 0;
        }

        /// <summary>
        /// Thực thi nghiệp vụ xóa một Danh mục khỏi hệ thống dựa trên ID.
        /// Lưu ý: Cần đảm bảo cơ sở dữ liệu xử lý an toàn khóa ngoại (không xóa nếu danh mục này đang chứa Phản ánh).
        /// </summary>
        /// <param name="id">Định danh của danh mục cần xóa.</param>
        /// <returns><c>true</c> nếu xóa thành công; ngược lại trả về <c>false</c>.</returns>
        public static async Task<bool> DeleteCategoryAsync(int id)
        {
            int result = await _categoryRepository.DeleteAsync(id);
            return result > 0;
        }
    }
}