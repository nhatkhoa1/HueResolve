using System;
using System.Collections.Generic;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể trung tâm lưu trữ thông tin phản ánh, kiến nghị hoặc sự cố đô thị
/// do người dân gửi đến hệ thống.
///
/// Đây là bảng nghiệp vụ cốt lõi của hệ thống, chứa toàn bộ dữ liệu đầu vào,
/// quá trình phân loại, điều phối, xử lý và phản hồi kết quả.
///
/// Dữ liệu trong thực thể này được sử dụng để:
/// - Tiếp nhận phản ánh từ người dân.
/// - Lưu mô tả nội dung, vị trí và thông tin liên hệ.
/// - Hỗ trợ AI phân loại tự động.
/// - Chuyển phản ánh đến đơn vị phụ trách.
/// - Theo dõi trạng thái xử lý trong suốt vòng đời phản ánh.
/// - Lưu kết quả xử lý và phản hồi từ cơ quan quản lý.
/// - Phục vụ tra cứu, giám sát, thống kê và báo cáo.
///
/// Một phản ánh có thể trải qua nhiều giai đoạn:
/// Pending → Assigned → InProgress → Resolved → Closed.
///
/// Thực thể này có liên kết với:
/// - Category (loại phản ánh)
/// - AdministrativeArea (địa bàn hành chính)
/// - Assignment (phân công xử lý)
/// - ReportStatusHistory (lịch sử trạng thái)
/// - ReportAttachment (tệp đính kèm minh chứng)
///
/// Đây là bảng trung tâm liên kết phần lớn các bảng nghiệp vụ khác.
/// </summary>
public class Report
{
    /// <summary>
    /// Khóa chính định danh duy nhất của phản ánh.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã tra cứu phản ánh dùng cho theo dõi và tra cứu tiến độ.
    /// </summary>
    public string TrackingCode { get; set; } = "";

    /// <summary>
    /// Tiêu đề tóm tắt nội dung phản ánh.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Mô tả chi tiết nội dung phản ánh.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Họ tên người gửi phản ánh.
    /// </summary>
    public string ReporterName { get; set; } = "";

    /// <summary>
    /// Số điện thoại liên hệ của người gửi phản ánh.
    /// </summary>
    public string ReporterPhone { get; set; } = "";

    /// <summary>
    /// Địa chỉ mô tả bằng văn bản của vị trí xảy ra phản ánh.
    /// </summary>
    public string AddressText { get; set; } = "";

    /// <summary>
    /// Tên phường hoặc xã nơi xảy ra phản ánh.
    /// </summary>
    public string WardName { get; set; } = "";

    /// <summary>
    /// Tên quận, huyện hoặc thành phố nơi xảy ra phản ánh.
    /// </summary>
    public string DistrictName { get; set; } = "";

    /// <summary>
    /// Vĩ độ vị trí phản ánh.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Kinh độ vị trí phản ánh.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết loại phản ánh.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết đơn vị hành chính.
    /// </summary>
    public int? AdministrativeAreaId { get; set; }

    /// <summary>
    /// Mức độ tin cậy AI khi phân loại phản ánh.
    /// </summary>
    public double? AiConfidence { get; set; }

    /// <summary>
    /// Trạng thái phân loại phản ánh.
    /// Ví dụ: Pending, Classified, Reviewed.
    /// </summary>
    public string ClassificationState { get; set; } = "Pending";

    /// <summary>
    /// Xác định phản ánh có cần duyệt thủ công hay không.
    /// </summary>
    public bool NeedsReview { get; set; } = false;

    /// <summary>
    /// Trạng thái xử lý hiện tại của phản ánh.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Nội dung kết quả hoặc ghi chú xử lý phản ánh.
    /// </summary>
    public string? ResolutionNote { get; set; }

    /// <summary>
    /// Phản hồi quản trị hoặc nhận xét bổ sung.
    /// </summary>
    public string? AdminFeedback { get; set; }

    /// <summary>
    /// Khóa định danh người dân gửi phản ánh nếu có liên kết tài khoản.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Đơn vị hiện đang được giao phụ trách xử lý phản ánh.
    /// </summary>
    public int? AssignedDepartmentId { get; set; }

    /// <summary>
    /// Thời điểm hoàn tất xử lý phản ánh theo UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Thời điểm tạo phản ánh theo UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất theo UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Điều hướng đến loại phản ánh.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Điều hướng đến đơn vị hành chính liên quan.
    /// </summary>
    public AdministrativeArea? AdministrativeArea { get; set; }

    /// <summary>
    /// Danh sách lịch sử thay đổi trạng thái của phản ánh.
    /// </summary>
    public ICollection<ReportStatusHistory> StatusHistories { get; set; }
        = new List<ReportStatusHistory>();

    /// <summary>
    /// Danh sách tệp đính kèm minh chứng của phản ánh.
    /// </summary>
    public ICollection<ReportAttachment> Attachments { get; set; }
        = new List<ReportAttachment>();

    /// <summary>
    /// Danh sách lịch sử phân công xử lý của phản ánh.
    /// </summary>
    public ICollection<Assignment> Assignments { get; set; }
        = new List<Assignment>();
}