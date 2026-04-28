using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu thông tin đơn vị hành chính phục vụ định danh khu vực tiếp nhận phản ánh,
/// quản lý phạm vi địa lý và hỗ trợ phân loại dữ liệu theo địa bàn.
///
/// Bảng này đại diện cho các khu vực hành chính như quận, huyện, thị xã,
/// phường, xã, thị trấn thuộc phạm vi quản lý của hệ thống.
///
/// Dữ liệu trong thực thể này được sử dụng để:
/// - Chuẩn hóa địa chỉ hành chính khi người dân gửi phản ánh.
/// - Liên kết phản ánh với đúng khu vực phụ trách xử lý.
/// - Hỗ trợ phân tuyến và điều phối phản ánh theo địa bàn.
/// - Phục vụ thống kê, báo cáo số lượng phản ánh theo khu vực.
/// - Hỗ trợ hiển thị bản đồ số (GIS), định vị và tra cứu theo vùng.
///
/// Một đơn vị hành chính có thể được liên kết với nhiều phản ánh khác nhau.
/// Thực thể này đóng vai trò dữ liệu danh mục dùng chung trong toàn hệ thống.
///
/// Ví dụ dữ liệu:
/// - DistrictName: Thành phố Huế
/// - WardName: Phường Vĩnh Ninh
/// </summary>
public class AdministrativeArea
{
    /// <summary>
    /// Khóa chính định danh duy nhất của đơn vị hành chính.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên quận, huyện, thị xã hoặc thành phố trực thuộc.
    /// Dùng để xác định cấp hành chính lớn hơn.
    /// </summary>
    public string DistrictName { get; set; } = "";

    /// <summary>
    /// Tên phường, xã hoặc thị trấn thuộc đơn vị hành chính cấp quận/huyện.
    /// Dùng để xác định địa bàn chi tiết của phản ánh.
    /// </summary>
    public string WardName { get; set; } = "";
}