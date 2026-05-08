using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Interface quản lý lịch sử cập nhật trạng thái (Timeline) của phản ánh.
    /// Phục vụ cho UC03 (Tra cứu tiến độ) và UC07 (Cập nhật trạng thái).
    /// </summary>
    public interface IReportStatusHistoryRepository
    {
        /// <summary>Lưu một mốc lịch sử mới vào cơ sở dữ liệu.</summary>
        Task<int> InsertAsync(ReportStatusHistory history);

        /// <summary>Lấy toàn bộ lịch sử (Timeline) của một phản ánh cụ thể, sắp xếp theo thời gian.</summary>
        Task<IEnumerable<ReportStatusHistory>> GetByReportIdAsync(Guid reportId);
    }
}