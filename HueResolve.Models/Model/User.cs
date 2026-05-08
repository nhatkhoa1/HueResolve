using System;

namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[Users].
    /// Chức năng: Quản lý thông tin tài khoản của 4 nhóm Actor (Admin, Operator, Handler, Citizen).
    /// Mối quan hệ: Liên kết khóa ngoại với bảng Roles (RoleId) và Departments (DepartmentId).
    /// </summary>
    public class User
    {
        /// <summary>Khóa chính (uniqueidentifier -> Guid)</summary>
        public Guid Id { get; set; }

        /// <summary>Tên đầy đủ của người dùng (Bắt buộc)</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Tên đăng nhập dùng để xác thực hệ thống (Bắt buộc, Unique)</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Mật khẩu đã được mã hóa bằng thuật toán BCrypt</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Khóa ngoại tham chiếu đến bảng Roles để phân quyền RBAC</summary>
        public int RoleId { get; set; }

        /// <summary>Thời gian tạo tài khoản chuẩn UTC</summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Số điện thoại liên hệ (Cho phép Null)</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Địa chỉ sinh sống/làm việc (Cho phép Null)</summary>
        public string? AddressText { get; set; }

        /// <summary>Khóa ngoại tham chiếu bảng Departments (Dành cho cán bộ/đơn vị xử lý, Cho phép Null)</summary>
        public int? DepartmentId { get; set; }

        /// <summary>Trạng thái hoạt động của tài khoản (1: Active, 0: Inactive)</summary>
        public bool IsActive { get; set; }
    }
}