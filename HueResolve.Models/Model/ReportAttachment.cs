using System;

namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[ReportAttachments].
    /// Chức năng: Quản lý các file đính kèm (hình ảnh/video) minh chứng.
    /// </summary>
    public class ReportAttachment
    {
        /// <summary>Khóa chính</summary>
        public Guid Id { get; set; }

        /// <summary>ID của Phản ánh chứa tệp đính kèm</summary>
        public Guid ReportId { get; set; }

        /// <summary>Tên file gốc do người dùng upload (VD: anh_hien_truong.jpg)</summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>Tên file được đổi lại khi lưu trên server (tránh trùng lặp)</summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>Đường dẫn tương đối đến file trên server (VD: uploads/reports/2026/...)</summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>Kiểu MIME của tệp (VD: image/jpeg, image/png, video/mp4)</summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>Thời gian tệp được upload</summary>
        public DateTime CreatedAtUtc { get; set; }
        /// <summary>
        ///     
        /// </summary>
        public byte[]? FileData { get; set; }

        /// <summary>Loại tệp đính kèm: "Citizen" (Công dân gửi) hoặc "Result" (Hiện trường gửi lại sau khi xử lý)</summary>
        public string AttachmentType { get; set; } = "Citizen";
    }
}