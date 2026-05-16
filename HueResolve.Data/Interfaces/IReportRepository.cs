using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Interface cốt lõi quản lý dữ liệu phản ánh hiện trường.
    /// Bao gồm các nghiệp vụ tiếp nhận, cập nhật trạng thái, tra cứu và phân trang.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>Lấy chi tiết một phản ánh bằng Id.</summary>
        Task<Report?> GetByIdAsync(Guid id);

        /// <summary>Tra cứu phản ánh công khai dành cho người dân thông qua TrackingCode (UC03).</summary>
        Task<Report?> GetByTrackingCodeAsync(string trackingCode);

        /// <summary>Lấy danh sách phản ánh cho Dashboard cán bộ (UC05, UC06).</summary>
        Task<IEnumerable<Report>> GetAllAsync(string? status = null, int? categoryId = null, string? search = null);

        /// <summary>Lấy danh sách phản ánh có phân trang và tổng số dòng.</summary>
        Task<(IEnumerable<Report> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? status, int? categoryId, string? search);

        /// <summary>
        /// Lấy tất cả phản ánh có tọa độ GPS để render marker và heatmap trên bản đồ (UC04, UC09).
        /// Hỗ trợ lọc theo CategoryId và Khoảng thời gian.
        /// </summary>
        Task<IEnumerable<Report>> GetForMapAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>Lưu một phản ánh mới (UC01).</summary>
        Task<int> InsertAsync(Report report);

        /// <summary>Cập nhật kết quả phân loại từ AI (UC02, UC05).</summary>
        Task<int> UpdateAiClassificationAsync(Guid reportId, int? categoryId, string classificationState, double confidence, bool needsReview);

        /// <summary>Cập nhật trạng thái tổng thể của phản ánh (UC07).</summary>
        Task<int> UpdateStatusAsync(Guid reportId, string newStatus, string? resolutionNote, DateTime? completedAtUtc);

        /// <summary>Cập nhật đơn vị chức năng được phân công (UC06).</summary>
        Task<int> UpdateAssignedDepartmentAsync(Guid reportId, int departmentId);

        /// <summary>Lưu phản hồi từ Admin hoặc Cán bộ IOC (UC05).</summary>
        Task<int> UpdateAdminFeedbackAsync(Guid reportId, string feedback);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="departmentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<int> UpdateAssignmentAsync(Guid reportId, int departmentId, string status);
        /// <summary>Cập nhật kết quả xử lý từ đơn vị thực địa và ghi log (Sử dụng Transaction).</summary>
        Task<bool> SubmitResultTransactionAsync(Guid reportId, string status, string resolutionNote, DateTime? completedAt, ReportStatusHistory history);
        /// <summary>
        /// Lấy danh sách phản ánh theo ID của người dân (Dành cho chức năng MyReports)
        /// </summary>
        Task<IEnumerable<Report>> GetByCustomerIdAsync(Guid customerId);
        
        /// <summary>Xóa hoàn toàn phản ánh khỏi hệ thống.</summary>
        Task<bool> DeleteAsync(Guid id);
    }
}