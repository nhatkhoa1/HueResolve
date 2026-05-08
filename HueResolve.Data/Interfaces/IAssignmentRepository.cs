using System;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Bổ sung thêm InsertAsync và GetLatestByReportIdAsync
    /// </summary>
    public interface IAssignmentRepository
    {
        /// <summary>Thực thi phân công bằng Transaction (Đã có)</summary>
        Task<bool> ExecuteAssignmentTransactionAsync(Assignment assignment, ReportStatusHistory history);

        /// <summary>Thêm mới Assignment độc lập</summary>
        Task<int> InsertAsync(Assignment assignment);

        /// <summary>Lấy thông tin phân công mới nhất của một phản ánh</summary>
        Task<Assignment?> GetLatestByReportIdAsync(Guid reportId);
    }
}