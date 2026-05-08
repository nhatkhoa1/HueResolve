using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Giao diện định nghĩa các thao tác dữ liệu cho tài khoản người dùng.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Lấy thông tin tài khoản để đăng nhập.</summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>Lấy chi tiết tài khoản qua Id.</summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>Lấy danh sách tài khoản có phân trang, lọc vai trò và tìm kiếm.</summary>
        Task<(IEnumerable<User> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, int? roleId, string? search);

        /// <summary>Thêm mới tài khoản (UC11).</summary>
        Task<int> InsertAsync(User user);

        /// <summary>Cập nhật trạng thái khóa/mở khóa tài khoản.</summary>
        Task<int> UpdateIsActiveAsync(Guid userId, bool isActive);
    }
}