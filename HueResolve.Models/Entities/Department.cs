using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin đơn vị, phòng ban hoặc cơ quan phụ trách xử lý phản ánh.
///
/// Bảng này đại diện cho các bộ phận chuyên môn trong hệ thống,
/// chịu trách nhiệm tiếp nhận, phân công và xử lý các phản ánh thuộc phạm vi phụ trách.
///
/// Dữ liệu phòng ban được sử dụng để:
/// - Quản lý danh mục đơn vị xử lý.
/// - Phân tuyến phản ánh đến đúng cơ quan phụ trách.
/// - Liên kết cán bộ, nhân sự với đơn vị công tác.
/// - Phục vụ điều phối xử lý theo chức năng chuyên môn.
/// - Hỗ trợ thống kê khối lượng và hiệu suất xử lý theo đơn vị.
///
/// Ví dụ đơn vị có thể gồm:
/// - Phòng Quản lý đô thị
/// - Công ty Môi trường đô thị
/// - Điện lực
/// - Ban quản lý hạ tầng kỹ thuật
///
/// Một phòng ban có thể có nhiều người dùng thuộc đơn vị đó,
/// tạo quan hệ một-nhiều giữa Department và User.
///
/// Đây là bảng dữ liệu danh mục dùng chung phục vụ nghiệp vụ điều phối.
/// </summary>
public class Department
{
    /// <summary>
    /// Khóa chính định danh duy nhất của đơn vị.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên đơn vị hoặc phòng ban phụ trách xử lý.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Mô tả chức năng, phạm vi phụ trách hoặc thông tin bổ sung của đơn vị.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Địa chỉ thư điện tử liên hệ của đơn vị.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của đơn vị.
    /// True: đang hoạt động.
    /// False: ngừng sử dụng hoặc bị vô hiệu hóa.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Danh sách người dùng thuộc đơn vị hiện tại.
    /// Thể hiện quan hệ một-nhiều với bảng User.
    /// </summary>
    public ICollection<User> Users { get; set; }
        = new List<User>();
}