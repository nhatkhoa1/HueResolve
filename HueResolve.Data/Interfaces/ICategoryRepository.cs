using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Giao diện định nghĩa các thao tác dữ liệu cơ bản cho bảng Categories.
    /// Bảng này lưu trữ danh mục các lĩnh vực sự cố đô thị (Giao thông, Môi trường, An ninh...).
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>Truy xuất tất cả các danh mục.</summary>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>Truy xuất một danh mục dựa trên Id.</summary>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>Lấy danh sách danh mục có phân trang và tìm kiếm.</summary>
        Task<(IEnumerable<Category> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search);

        /// <summary>Thêm mới một danh mục sự cố.</summary>
        Task<int> InsertAsync(Category category);

        /// <summary>Cập nhật thông tin danh mục sự cố.</summary>
        Task<int> UpdateAsync(Category category);

        /// <summary>Xóa một danh mục khỏi hệ thống.</summary>
        Task<int> DeleteAsync(int id);
    }
}