namespace HueResolve.Models.Model
{
    /// <summary>
    /// Lớp thực thể đại diện cho bảng [dbo].[AdministrativeAreas].
    /// Chức năng: Lưu trữ danh mục các đơn vị hành chính (Phường/Xã/Thành phố) hỗ trợ bản đồ GIS.
    /// </summary>
    public class AdministrativeArea
    {
        /// <summary>Khóa chính tự tăng</summary>
        public int Id { get; set; }

        /// <summary>Tên Quận/Huyện/Thành phố (VD: Thành phố Huế)</summary>
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>Tên Phường/Xã (VD: Phường Vỹ Dạ)</summary>
        public string WardName { get; set; } = string.Empty;
    }
}