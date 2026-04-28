using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin người dùng trong hệ thống.
///
/// Bảng này quản lý các tài khoản tham gia vận hành hoặc sử dụng hệ thống,
/// bao gồm người dân gửi phản ánh, cán bộ xử lý và quản trị viên.
///
/// Dữ liệu người dùng được sử dụng để:
/// - Quản lý tài khoản đăng nhập.
/// - Xác thực danh tính và phân quyền truy cập.
/// - Liên kết người dùng với vai trò nghiệp vụ.
/// - Liên kết cán bộ với đơn vị công tác.
/// - Lưu thông tin liên hệ phục vụ xử lý phản ánh.
/// - Ghi nhận chủ thể thực hiện thao tác trong hệ thống.
///
/// Các nhóm người dùng có thể gồm:
/// - Công dân (Customer)
/// - Cán bộ xử lý (Staff)
/// - Quản trị hệ thống (Admin)
///
/// Người dùng được liên kết với:
/// - Role để xác định quyền hạn.
/// - Department để xác định đơn vị công tác (nếu có).
///
/// Đây là bảng nghiệp vụ cốt lõi phục vụ xác thực và quản lý người dùng.
/// </summary>
public class User
{
    /// <summary>
    /// Khóa chính định danh duy nhất của người dùng.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Họ và tên người dùng.
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// Tên đăng nhập dùng cho xác thực hệ thống.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Mật khẩu đã được mã hóa hoặc băm để lưu trữ an toàn.
    /// </summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>
    /// Khóa ngoại liên kết vai trò người dùng.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết đơn vị công tác của người dùng.
    /// Có thể null nếu người dùng không thuộc đơn vị nội bộ.
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của người dùng.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Địa chỉ liên hệ của người dùng.
    /// </summary>
    public string? AddressText { get; set; }

    /// <summary>
    /// Thời điểm tạo tài khoản theo UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng liên kết đến vai trò người dùng.
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng liên kết đến đơn vị công tác.
    /// </summary>
    public Department? Department { get; set; }
}