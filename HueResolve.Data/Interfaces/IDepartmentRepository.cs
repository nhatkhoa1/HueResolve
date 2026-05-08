using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Giao diện định nghĩa các thao tác dữ liệu cho bảng Departments.
    /// Hỗ trợ quản lý vòng đời đơn vị xử lý.
    /// </summary>
    public interface IDepartmentRepository
    {
        /// <summary>Truy vấn một đơn vị theo Id.</summary>
        Task<Department?> GetByIdAsync(int id);

        /// <summary>Lấy toàn bộ danh sách đơn vị đang hoạt động.</summary>
        Task<IEnumerable<Department>> GetAllActiveAsync();

        /// <summary>Lấy danh sách đơn vị có phân trang và tìm kiếm.</summary>
        Task<(IEnumerable<Department> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search);

        /// <summary>Thêm mới đơn vị xử lý.</summary>
        Task<int> InsertAsync(Department department);

        /// <summary>Cập nhật thông tin đơn vị.</summary>
        Task<int> UpdateAsync(Department department);

        /// <summary>Cập nhật trạng thái khóa/mở khóa đơn vị.</summary>
        Task<int> UpdateIsActiveAsync(int id, bool isActive);

        /// <summary>Xóa vĩnh viễn bản ghi đơn vị khỏi hệ thống.</summary>
        Task<int> DeleteAsync(int id);
    }
}