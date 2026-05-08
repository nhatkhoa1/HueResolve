using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Dịch vụ chuyên trách xử lý dữ liệu không gian (GIS) phục vụ cho bản đồ số.
    /// </summary>
    public static class MapService
    {
        private static IReportRepository _reportRepository = default!;

        /// <summary>Khởi tạo kết nối Repository cho MapService.</summary>
        public static void Initialize(string connectionString)
        {
            _reportRepository = new ReportRepository(connectionString);
        }

        /// <summary>
        /// Truy xuất danh sách các phản ánh có chứa tọa độ GPS hợp lệ.
        /// Hỗ trợ lọc theo Danh mục (Lĩnh vực) và Khoảng thời gian.
        /// </summary>
        public static async Task<IEnumerable<Report>> GetMapDataAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _reportRepository.GetForMapAsync(categoryId, fromDate, toDate);
        }
    }
}