using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin tệp đính kèm liên quan đến phản ánh.
///
/// Bảng này quản lý các tệp minh chứng do người dân hoặc cán bộ
/// tải lên trong quá trình tiếp nhận và xử lý phản ánh.
///
/// Tệp đính kèm có thể bao gồm:
/// - Hình ảnh hiện trường
/// - Video minh chứng
/// - Tài liệu liên quan
/// - Hình ảnh sau xử lý
///
/// Dữ liệu trong thực thể này được sử dụng để:
/// - Lưu minh chứng cho nội dung phản ánh.
/// - Hỗ trợ xác minh thực tế hiện trường.
/// - Phục vụ quá trình xử lý và kiểm tra kết quả.
/// - Cung cấp tài liệu tham chiếu cho đơn vị phụ trách.
/// - Lưu trữ hồ sơ điện tử gắn với phản ánh.
///
/// Một phản ánh có thể có nhiều tệp đính kèm,
/// tạo quan hệ một-nhiều giữa Report và ReportAttachment.
///
/// Thực thể này chỉ lưu metadata của tệp,
/// không lưu trực tiếp nội dung file nhị phân.
/// </summary>
public class ReportAttachment
{
    /// <summary>
    /// Khóa chính định danh duy nhất của tệp đính kèm.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa ngoại tham chiếu đến phản ánh sở hữu tệp đính kèm.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Tên tệp gốc do người dùng tải lên.
    /// </summary>
    public string OriginalFileName { get; set; } = "";

    /// <summary>
    /// Tên tệp lưu trữ nội bộ trên hệ thống.
    /// Thường được đổi tên để tránh trùng lặp.
    /// </summary>
    public string StoredFileName { get; set; } = "";

    /// <summary>
    /// Đường dẫn tương đối đến vị trí lưu trữ tệp.
    /// </summary>
    public string RelativePath { get; set; } = "";

    /// <summary>
    /// Kiểu nội dung của tệp (MIME type),
    /// ví dụ image/jpeg, image/png, video/mp4.
    /// </summary>
    public string ContentType { get; set; } = "";

    /// <summary>
    /// Thời điểm tạo bản ghi tệp đính kèm theo UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng liên kết đến phản ánh tương ứng.
    /// </summary>
    public Report? Report { get; set; }
}