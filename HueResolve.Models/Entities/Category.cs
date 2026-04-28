using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.Entities;

/// <summary>
/// Thực thể lưu danh mục loại phản ánh trong hệ thống.
///
/// Bảng này dùng để định nghĩa các nhóm, lĩnh vực hoặc chủ đề
/// mà phản ánh của người dân có thể thuộc về.
///
/// Dữ liệu danh mục được sử dụng để:
/// - Phân loại phản ánh theo từng lĩnh vực nghiệp vụ.
/// - Hỗ trợ AI nhận diện và gán loại phản ánh tự động.
/// - Điều phối phản ánh đến đúng đơn vị chuyên trách.
/// - Thống kê, báo cáo số lượng phản ánh theo từng nhóm.
/// - Phục vụ tra cứu, lọc dữ liệu và tìm kiếm theo loại vấn đề.
///
/// Ví dụ loại phản ánh có thể gồm:
/// - Hạ tầng giao thông
/// - Điện chiếu sáng
/// - Vệ sinh môi trường
/// - Trật tự đô thị
/// - Cấp thoát nước
///
/// Một loại phản ánh có thể được liên kết với nhiều phản ánh khác nhau,
/// tạo thành quan hệ một-nhiều giữa Category và Report.
///
/// Đây là bảng dữ liệu danh mục dùng chung cho toàn hệ thống.
/// </summary>
public class Category
{
    /// <summary>
    /// Khóa chính định danh duy nhất của loại phản ánh.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã định danh ngắn của loại phản ánh,
    /// dùng cho chuẩn hóa dữ liệu và xử lý nghiệp vụ.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// Tên hiển thị của loại phản ánh.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Danh sách các phản ánh thuộc loại hiện tại.
    /// Thể hiện quan hệ một-nhiều với bảng Report.
    /// </summary>
    public ICollection<Report> Reports { get; set; }
        = new List<Report>();
}