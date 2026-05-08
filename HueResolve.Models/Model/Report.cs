using System;

namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể CỐT LÕI đại diện cho bảng [dbo].[Reports].
    /// Chức năng: Lưu trữ toàn bộ thông tin chi tiết của một phản ánh hiện trường.
    /// </summary>
    public class Report
    {
        /// <summary>Khóa chính</summary>
        public Guid Id { get; set; }

        /// <summary>Mã tra cứu công khai (VD: HUE-20240105-0005)</summary>
        public string TrackingCode { get; set; } = string.Empty;

        /// <summary>Tiêu đề ngắn gọn của phản ánh</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Nội dung chi tiết sự cố</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Họ tên người gửi</summary>
        public string ReporterName { get; set; } = string.Empty;

        /// <summary>Số điện thoại người gửi</summary>
        public string ReporterPhone { get; set; } = string.Empty;

        /// <summary>Địa chỉ văn bản nơi xảy ra sự cố</summary>
        public string AddressText { get; set; } = string.Empty;

        /// <summary>Phường/Xã nơi xảy ra sự cố</summary>
        public string WardName { get; set; } = string.Empty;

        /// <summary>Quận/Huyện/Thành phố nơi xảy ra sự cố</summary>
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>Vĩ độ GPS phục vụ Mapbox GL JS</summary>
        public double Latitude { get; set; }

        /// <summary>Kinh độ GPS phục vụ Mapbox GL JS</summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu bảng Categories (Cho phép Null).
        /// Null khi phản ánh vừa tạo, chưa được AI hoặc cán bộ phân loại.
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu bảng AdministrativeAreas (Cho phép Null).
        /// Null khi chưa xác định được đơn vị hành chính chính xác.
        /// </summary>
        public int? AdministrativeAreaId { get; set; }

        /// <summary>Điểm tin cậy do mô hình AI PhoBERT đánh giá (0.0 đến 1.0)</summary>
        public double? AiConfidence { get; set; }

        /// <summary>Trạng thái phân loại của AI (VD: Pending, Success, LowConfidence)</summary>
        public string ClassificationState { get; set; } = string.Empty;

        /// <summary>Cờ đánh dấu cần Cán bộ IOC duyệt lại</summary>
        public bool NeedsReview { get; set; }

        /// <summary>Trạng thái hiện tại của quy trình (TiepNhan, DangXuLy, HoanThanh, TuChoi)</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Ghi chú kết quả xử lý từ Đơn vị chức năng (Cho phép Null)</summary>
        public string? ResolutionNote { get; set; }

        /// <summary>Thời gian hoàn tất xử lý (Cho phép Null)</summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>Thời gian tạo phản ánh</summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Thời gian cập nhật gần nhất</summary>
        public DateTime UpdatedAtUtc { get; set; }

        /// <summary>ID tài khoản công dân gửi (Cho phép Null nếu nặc danh)</summary>
        public Guid? CustomerId { get; set; }

        /// <summary>Khóa ngoại tham chiếu đơn vị xử lý (Cho phép Null)</summary>
        public int? AssignedDepartmentId { get; set; }

        /// <summary>Phản hồi từ Admin hoặc Cán bộ IOC (Cho phép Null)</summary>
        public string? AdminFeedback { get; set; }
    }
}