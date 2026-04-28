using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin phân công xử lý phản ánh trong hệ thống.
///
/// Bảng này ghi nhận việc một phản ánh được giao cho cá nhân,
/// cán bộ hoặc đơn vị phụ trách xử lý.
///
/// Dữ liệu phân công được sử dụng để:
/// - Chỉ định người hoặc bộ phận chịu trách nhiệm xử lý phản ánh.
/// - Theo dõi thời điểm tiếp nhận và giao việc.
/// - Ghi chú nội dung chỉ đạo, hướng dẫn hoặc yêu cầu xử lý.
/// - Phục vụ giám sát tiến độ và truy vết trách nhiệm xử lý.
/// - Hỗ trợ điều phối và tái phân công khi cần.
///
/// Một phản ánh có thể có nhiều lần phân công trong vòng đời xử lý,
/// ví dụ chuyển tuyến, đổi đơn vị phụ trách hoặc giao lại xử lý.
///
/// Thực thể này đóng vai trò lưu lịch sử điều phối nghiệp vụ
/// và liên kết giữa phản ánh với đối tượng được giao xử lý.
/// </summary>
public class Assignment
{
    /// <summary>
    /// Khóa chính định danh duy nhất của bản ghi phân công.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa ngoại tham chiếu đến phản ánh được phân công xử lý.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Khóa định danh người dùng hoặc cán bộ được giao xử lý phản ánh.
    /// </summary>
    public Guid AssigneeId { get; set; }

    /// <summary>
    /// Ghi chú nội dung phân công, chỉ đạo hoặc lưu ý trong quá trình giao việc.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Thời điểm phản ánh được phân công xử lý theo giờ UTC.
    /// </summary>
    public DateTime AssignedAtUtc { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng liên kết tới phản ánh tương ứng.
    /// </summary>
    public Report? Report { get; set; }
}