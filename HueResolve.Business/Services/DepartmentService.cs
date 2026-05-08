using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Lớp dịch vụ tĩnh (Static Service) chuyên phụ trách các quy tắc nghiệp vụ (Business Logic) 
    /// liên quan đến thực thể Đơn vị xử lý (Sở/Ban/Ngành/Công ty).
    /// Đây là cầu nối giữa tầng UI (Controller) và tầng Data Access (Repository).
    /// </summary>
    public static class DepartmentService
    {
        private static IDepartmentRepository _departmentRepository = default!;

        /// <summary>
        /// Khởi tạo dịch vụ và tiêm chuỗi kết nối vào Repository tương ứng.
        /// Lưu ý: Hàm này phải được gọi một lần duy nhất lúc khởi động ứng dụng (trong Program.cs).
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server hợp lệ.</param>
        public static void Initialize(string connectionString)
        {
            _departmentRepository = new DepartmentRepository(connectionString);
        }

        /// <summary>
        /// Truy xuất thông tin chi tiết của một Đơn vị cụ thể dựa trên ID.
        /// Thường dùng cho các màn hình Xem chi tiết hoặc load dữ liệu lên form Sửa.
        /// </summary>
        /// <param name="id">Mã định danh của Đơn vị.</param>
        /// <returns>Đối tượng <see cref="Department"/> nếu tìm thấy; ngược lại là <c>null</c>.</returns>
        public static async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _departmentRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Lấy danh sách tất cả các Đơn vị đang trong trạng thái "Hoạt động" (IsActive = true).
        /// Nghiệp vụ chính: Dùng để đổ dữ liệu vào các thanh Dropdown list (Select) khi Cán bộ tiến hành Phân công phản ánh.
        /// Các đơn vị đã bị khóa sẽ không được hiển thị để tránh lỗi logic.
        /// </summary>
        /// <returns>Tập hợp <see cref="IEnumerable{Department}"/> chứa các đơn vị đang hoạt động.</returns>
        public static async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            return await _departmentRepository.GetAllActiveAsync();
        }

        /// <summary>
        /// Truy xuất danh sách Đơn vị có cơ chế phân trang và hỗ trợ tìm kiếm linh hoạt.
        /// Sử dụng chính cho màn hình Quản trị Đơn vị của Admin (Giao diện Table).
        /// </summary>
        /// <param name="page">Số trang hiện tại (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số dòng tối đa hiển thị trên một trang.</param>
        /// <param name="search">Từ khóa tìm kiếm (theo tên, số điện thoại, email...). Null nếu không lọc.</param>
        /// <returns>Một Tuple chứa: Tập dữ liệu của trang hiện tại (<c>Data</c>) và Tổng số đơn vị thỏa điều kiện (<c>TotalCount</c>).</returns>
        public static async Task<(IEnumerable<Department> Data, int TotalCount)> GetPagedDepartmentsAsync(int page, int pageSize, string? search)
        {
            return await _departmentRepository.GetPagedAsync(page, pageSize, search);
        }

        /// <summary>
        /// Thực thi nghiệp vụ tạo mới một Đơn vị xử lý.
        /// Tự động thiết lập trạng thái mặc định của đơn vị mới là "Hoạt động" (IsActive = true).
        /// </summary>
        /// <param name="department">Đối tượng <see cref="Department"/> chứa thông tin cần tạo.</param>
        /// <returns><c>true</c> nếu quá trình ghi dữ liệu thành công; ngược lại <c>false</c>.</returns>
        public static async Task<bool> CreateDepartmentAsync(Department department)
        {
            department.IsActive = true;
            int result = await _departmentRepository.InsertAsync(department);
            return result > 0;
        }

        /// <summary>
        /// Thực thi nghiệp vụ cập nhật thông tin chung của một Đơn vị đã tồn tại (Tên, Liên hệ, Mô tả).
        /// Không làm thay đổi trạng thái Khóa/Mở khóa.
        /// </summary>
        /// <param name="department">Đối tượng mang thông tin mới.</param>
        /// <returns><c>true</c> nếu cập nhật thành công.</returns>
        public static async Task<bool> UpdateDepartmentAsync(Department department)
        {
            int result = await _departmentRepository.UpdateAsync(department);
            return result > 0;
        }

        /// <summary>
        /// Cập nhật nhanh trạng thái Hoạt động (Khóa hoặc Mở khóa) của một Đơn vị.
        /// Khi bị khóa (isActive = false), đơn vị sẽ không thể tiếp nhận thêm sự cố phản ánh mới.
        /// </summary>
        /// <param name="id">ID của đơn vị cần thao tác.</param>
        /// <param name="isActive">Giá trị cờ boolean: true = Mở khóa, false = Khóa.</param>
        /// <returns><c>true</c> nếu chuyển đổi trạng thái thành công.</returns>
        public static async Task<bool> SetDepartmentActiveAsync(int id, bool isActive)
        {
            int result = await _departmentRepository.UpdateIsActiveAsync(id, isActive);
            return result > 0;
        }

        /// <summary>
        /// Thực hiện xóa vĩnh viễn (Hard Delete) một Đơn vị xử lý khỏi hệ thống.
        /// Cảnh báo: Việc xóa sẽ thất bại và quăng ngoại lệ ở tầng CSDL nếu đơn vị này đang bị ràng buộc
        /// bởi các Phản ánh hoặc Nhân viên đang thuộc về nó.
        /// </summary>
        /// <param name="id">Mã số định danh của đơn vị.</param>
        /// <returns><c>true</c> nếu bản ghi được loại bỏ hoàn toàn.</returns>
        public static async Task<bool> DeleteDepartmentAsync(int id)
        {
            int result = await _departmentRepository.DeleteAsync(id);
            return result > 0;
        }
    }
}