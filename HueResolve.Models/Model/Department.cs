namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[Departments].
    /// Đã cập nhật thêm thuộc tính PhoneNumber.
    /// </summary>
    public class Department
    {
        /// <summary>Khóa chính tự tăng</summary>
        public int Id { get; set; }

        /// <summary>Tên cơ quan/đơn vị (Bắt buộc)</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả chức năng, nhiệm vụ của đơn vị</summary>
        public string? Description { get; set; }

        /// <summary>Email liên hệ để nhận cảnh báo SLA/thông báo</summary>
        public string? Email { get; set; }

        /// <summary>Số điện thoại liên hệ/Hotline xử lý sự cố</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Trạng thái hoạt động</summary>
        public bool IsActive { get; set; }

        /// <summary>Phân loại đơn vị (Sở ban ngành, Doanh nghiệp...)</summary>
        public string? Type { get; set; }
    }
}