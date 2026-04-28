using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu lịch sử thay đổi trạng thái xử lý của phản ánh.
///
/// Bảng này ghi nhận toàn bộ diễn biến trạng thái của một phản ánh
/// trong suốt vòng đời xử lý, thay vì chỉ lưu trạng thái hiện tại.
///
/// Dữ liệu trong thực thể này được sử dụng để:
/// - Theo dõi tiến trình xử lý phản ánh theo từng giai đoạn.
/// - Lưu vết các lần cập nhật trạng thái.
/// - Truy xuất lịch sử nghiệp vụ và phục vụ kiểm tra.
/// - Xác định trách nhiệm cập nhật xử lý.
/// - Phục vụ giám sát, báo cáo và truy vết thay đổi.
///
/// Một phản ánh có thể có nhiều bản ghi lịch sử trạng thái,
/// tạo quan hệ một-nhiều giữa Report và ReportStatusHistory.
///
/// Ví dụ chuỗi trạng thái có thể gồm:
/// Pending
/// Assigned
/// InProgress
/// Resolved
/// Closed
///
/// Đây là bảng lưu nhật ký tiến trình xử lý phản ánh.
/// </summary>
public class ReportStatusHistory
{
    /// <summary>
    /// Khóa chính định danh duy nhất của bản ghi lịch sử trạng thái.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa ngoại tham chiếu đến phản ánh liên quan.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Trạng thái xử lý của phản ánh tại thời điểm cập nhật.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Ghi chú bổ sung mô tả nội dung cập nhật trạng thái.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Tên người thực hiện cập nhật trạng thái.
    /// </summary>
    public string? UpdatedByName { get; set; }

    /// <summary>
    /// Thời điểm ghi nhận cập nhật trạng thái theo UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng liên kết đến phản ánh tương ứng.
    /// </summary>
    public Report? Report { get; set; }
}