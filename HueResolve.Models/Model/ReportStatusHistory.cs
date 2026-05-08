using System;

namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[ReportStatusHistories].
    /// Chức năng: Quản lý Timeline 4 bước của phản ánh, giúp minh bạch tiến độ.
    /// </summary>
    public class ReportStatusHistory
    {
        /// <summary>Khóa chính</summary>
        public Guid Id { get; set; }

        /// <summary>ID của Phản ánh liên quan</summary>
        public Guid ReportId { get; set; }

        /// <summary>Trạng thái được cập nhật (VD: TiepNhan, DangXuLy, HoanThanh, TuChoi)</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Nội dung chi tiết lý do chuyển trạng thái (Cho phép Null)</summary>
        public string? Note { get; set; }

        /// <summary>Tên của cán bộ thực hiện cập nhật (Cho phép Null)</summary>
        public string? UpdatedByName { get; set; }

        /// <summary>Thời gian cập nhật trạng thái</summary>
        public DateTime CreatedAtUtc { get; set; }
    }
}