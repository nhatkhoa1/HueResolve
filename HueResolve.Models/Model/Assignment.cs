using System;

namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[Assignments].
    /// Đã sửa đổi AssignedByUserId thành AssigneeId để khớp chuẩn với Database Schema.
    /// </summary>
    public class Assignment
    {
        /// <summary>Khóa chính của bản ghi phân công</summary>
        public Guid Id { get; set; }

        /// <summary>Khóa ngoại liên kết tới bảng Reports</summary>
        public Guid ReportId { get; set; }

        /// <summary>Khóa ngoại liên kết tới bảng Departments (Đơn vị được phân công)</summary>
        public int DepartmentId { get; set; }

        /// <summary>Khóa ngoại liên kết tới bảng Users (Người thực hiện phân công)</summary>
        public Guid AssigneeId { get; set; }

        /// <summary>Thời gian thực hiện phân công chuẩn UTC</summary>
        public DateTime AssignedAtUtc { get; set; }

        /// <summary>Ghi chú hoặc chỉ đạo từ người phân công</summary>
        public string? Note { get; set; }
    }
}