namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[Roles].
    /// Chức năng: Định nghĩa các vai trò (RBAC) để phân quyền trong hệ thống (VD: Admin, Officer, Customer).
    /// </summary>
    public class Role
    {
        /// <summary>Khóa chính tự tăng</summary>
        public int Id { get; set; }

        /// <summary>Tên vai trò (Bắt buộc, Unique)</summary>
        public string Name { get; set; } = string.Empty;
    }
}