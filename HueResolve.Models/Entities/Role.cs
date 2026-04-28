using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin vai trò người dùng trong hệ thống.
///
/// Bảng này định nghĩa các nhóm quyền hoặc vai trò truy cập
/// dùng để phân quyền và kiểm soát chức năng sử dụng.
///
/// Dữ liệu vai trò được sử dụng để:
/// - Phân quyền truy cập hệ thống.
/// - Xác định chức năng người dùng được phép sử dụng.
/// - Kiểm soát phạm vi thao tác theo từng nhóm quyền.
/// - Hỗ trợ xác thực và ủy quyền nghiệp vụ.
/// - Liên kết người dùng với vai trò tương ứng.
///
/// Ví dụ vai trò có thể gồm:
/// - Admin
/// - Staff
/// - Customer
///
/// Một vai trò có thể được gán cho nhiều người dùng,
/// tạo quan hệ một-nhiều giữa Role và User.
///
/// Đây là bảng danh mục phục vụ quản lý phân quyền hệ thống.
/// </summary>
public class Role
{
    /// <summary>
    /// Khóa chính định danh duy nhất của vai trò.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên vai trò hoặc nhóm quyền người dùng.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Danh sách người dùng thuộc vai trò hiện tại.
    /// Thể hiện quan hệ một-nhiều với bảng User.
    /// </summary>
    public ICollection<User> Users { get; set; }
        = new List<User>();
}