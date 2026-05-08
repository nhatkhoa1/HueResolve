namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[Categories].
    /// Chức năng: Lưu danh mục lĩnh vực sự cố phục vụ cho AI PhoBERT phân loại.
    /// Đã tích hợp cả Code (bản gốc) và Description (mới thêm).
    /// </summary>
    public class Category
    {
        /// <summary>Khóa chính tự tăng</summary>
        public int Id { get; set; }

        /// <summary>Mã danh mục (VD: 'GiaoThong' - Unique)</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Tên hiển thị của danh mục</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả từ khóa định hướng cho AI</summary>
        public string? Description { get; set; }
    }
}