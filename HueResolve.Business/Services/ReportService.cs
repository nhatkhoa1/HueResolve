using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;
using System.IO;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Lớp dịch vụ tĩnh trung tâm xử lý toàn bộ logic nghiệp vụ cho Phản ánh.
    /// Chịu trách nhiệm điều phối dữ liệu giữa tầng Giao diện và tầng Truy xuất dữ liệu.
    /// </summary>
    public static class ReportService
    {
        private static IReportRepository _reportRepository = default!;
        private static IReportStatusHistoryRepository _historyRepository = default!;
        private static IReportAttachmentRepository _attachmentRepository = default!;
        private static IAssignmentRepository _assignmentRepository = default!;

        /// <summary>
        /// Khởi tạo các Repository cần thiết.
        /// </summary>
        public static void Initialize(string connectionString)
        {
            _reportRepository = new ReportRepository(connectionString);
            _historyRepository = new ReportStatusHistoryRepository(connectionString);
            _attachmentRepository = new ReportAttachmentRepository(connectionString);
            _assignmentRepository = new AssignmentRepository(connectionString);
        }

        /// <summary>
        /// Lấy thông tin phản ánh theo Id.
        /// </summary>
        public static async Task<Report?> GetReportByIdAsync(Guid id)
        {
            return await _reportRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Tìm kiếm phản ánh công khai qua mã Tracking Code.
        /// </summary>
        public static async Task<Report?> GetByTrackingCodeAsync(string trackingCode)
        {
            /// Giả sử bạn đã thêm hàm này vào Repository
            return await _reportRepository.GetByTrackingCodeAsync(trackingCode);
        }
        /// <summary>
        /// Lấy danh sách phản ánh cá nhân của một người dân.
        /// </summary>
        public static async Task<IEnumerable<Report>> GetByCustomerIdAsync(Guid customerId)
        {
            var all = await _reportRepository.GetAllAsync(); /// Tạm thời lấy hết rồi lọc (có thể tối ưu sau)
            return all.Where(r => r.CustomerId == customerId).OrderByDescending(r => r.CreatedAtUtc);
        }

        /// <summary>
        /// Lấy danh sách phản ánh có phân trang và lọc dữ liệu.
        /// </summary>
        public static async Task<(IEnumerable<Report> Data, int TotalCount)> GetPagedReportsAsync(int page, int pageSize, string? status, int? categoryId, string? search)
        {
            return await _reportRepository.GetPagedAsync(page, pageSize, status, categoryId, search);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách phản ánh.
        /// </summary>
        public static async Task<IEnumerable<Report>> GetAllReportsAsync(string? status = null, int? categoryId = null, string? search = null)
        {
            return await _reportRepository.GetAllAsync(status, categoryId, search);
        }

        /// <summary>
        /// Lấy N phản ánh mới nhất để hiển thị Dashboard.
        /// </summary>
        public static async Task<IEnumerable<Report>> GetRecentReportsAsync(int limit)
        {
            var reports = await _reportRepository.GetAllAsync();
            return reports.OrderByDescending(r => r.CreatedAtUtc).Take(limit);
        }

        /// <summary>
        /// Lấy dữ liệu phản ánh có tọa độ để vẽ lên bản đồ GIS.
        /// </summary>
        public static async Task<IEnumerable<Report>> GetReportsForMapAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _reportRepository.GetForMapAsync(categoryId, fromDate, toDate);
        }

        /// <summary>
        /// Xử lý tạo mới một phản ánh và ghi log tiếp nhận.
        /// </summary>
        public static async Task<bool> CreateReportAsync(Report report)
        {
            report.Id = Guid.NewGuid();
            report.TrackingCode = GenerateTrackingCode();
            report.Status = "TiepNhan";
            report.CreatedAtUtc = DateTime.UtcNow;
            report.UpdatedAtUtc = report.CreatedAtUtc;
            report.ClassificationState = "Pending";

            int result = await _reportRepository.InsertAsync(report);

            if (result > 0)
            {
                var history = new ReportStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ReportId = report.Id,
                    Status = "TiepNhan",
                    Note = "Hệ thống đã tiếp nhận phản ánh.",
                    UpdatedByName = report.ReporterName,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _historyRepository.InsertAsync(history);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cập nhật kết quả phân loại từ AI.
        /// </summary>
        public static async Task<bool> UpdateAiClassificationAsync(Guid reportId, int? categoryId, string classificationState, double confidence, bool needsReview)
        {
            int result = await _reportRepository.UpdateAiClassificationAsync(reportId, categoryId, classificationState, confidence, needsReview);
            return result > 0;
        }

        /// <summary>
        /// Chức năng chính: Cập nhật trạng thái tổng quát và ghi Timeline.
        /// </summary>
        public static async Task<bool> UpdateReportStatusAsync(Guid reportId, string newStatus, string? resolutionNote, string updatedByName)
        {
            DateTime? completedAt = newStatus == "HoanThanh" ? DateTime.UtcNow : null;
            int result = await _reportRepository.UpdateStatusAsync(reportId, newStatus, resolutionNote, completedAt);

            if (result > 0)
            {
                var history = new ReportStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ReportId = reportId,
                    Status = newStatus,
                    Note = resolutionNote ?? $"Cập nhật trạng thái: {newStatus}",
                    UpdatedByName = updatedByName,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _historyRepository.InsertAsync(history);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lưu phản hồi dành cho người dân.
        /// </summary>
        public static async Task<bool> SaveAdminFeedbackAsync(Guid reportId, string feedback)
        {
            int result = await _reportRepository.UpdateAdminFeedbackAsync(reportId, feedback);
            return result > 0;
        }

        /// <summary>
        /// Từ chối phản ánh không hợp lệ.
        /// </summary>
        public static async Task<bool> RejectReportAsync(Guid reportId, string reason, string updatedByName)
        {
            int result = await _reportRepository.UpdateStatusAsync(reportId, "TuChoi", reason, null);

            if (result > 0)
            {
                var history = new ReportStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ReportId = reportId,
                    Status = "TuChoi",
                    Note = string.IsNullOrWhiteSpace(reason) ? "Từ chối xử lý sự cố." : $"Lý do từ chối: {reason}",
                    UpdatedByName = updatedByName,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _historyRepository.InsertAsync(history);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lấy lịch sử thay đổi của phản ánh.
        /// </summary>
        public static async Task<IEnumerable<ReportStatusHistory>> GetHistoryByReportIdAsync(Guid reportId)
        {
            return await _historyRepository.GetByReportIdAsync(reportId);
        }

        /// <summary>
        /// Lấy danh sách tệp đính kèm.
        /// </summary>
        public static async Task<IEnumerable<ReportAttachment>> GetAttachmentsByReportIdAsync(Guid reportId)
        {
            return await _attachmentRepository.GetByReportIdAsync(reportId);
        }

        /// <summary>
        /// Thu thập dữ liệu thống kê Dashboard.
        /// </summary>
        public static async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var reports = await _reportRepository.GetAllAsync();
            var stat = new DashboardStats
            {
                TotalReports = reports.Count(),
                Pending = reports.Count(r => r.Status == "TiepNhan"),
                Processing = reports.Count(r => r.Status == "DangXuLy"),
                PendingApproval = reports.Count(r => r.Status == "ChoDuyetKq"),
                Completed = reports.Count(r => r.Status == "HoanThanh"),
                Rejected = reports.Count(r => r.Status == "TuChoi"),
                NeedsReview = reports.Count(r => r.NeedsReview),
                GiaoThong = reports.Count(r => r.CategoryId == 1),
                MoiTruong = reports.Count(r => r.CategoryId == 2),
                HaTang = reports.Count(r => r.CategoryId == 3),
                AnNinh = reports.Count(r => r.CategoryId == 4),
                Khac = reports.Count(r => r.CategoryId != 1 && r.CategoryId != 2 && r.CategoryId != 3 && r.CategoryId != 4)
            };
            return stat;
        }

        /// <summary>
        /// Lưu thông tin metadata của file đính kèm do Handler upload sau khi xử lý thực địa.
        /// File vật lý đã được lưu vào disk tại tầng Controller trước khi gọi hàm này.
        /// </summary>
        /// <param name="reportId">ID của phản ánh cần đính kèm file.</param>
        /// <param name="attachment">Đối tượng chứa metadata file (tên gốc, đường dẫn, MIME type...).</param>
        /// <returns><c>true</c> nếu lưu metadata thành công vào CSDL; ngược lại <c>false</c>.</returns>
        public static async Task<bool> SaveAttachmentAsync(Guid reportId, ReportAttachment attachment)
        {
            attachment.ReportId = reportId;
            int result = await _attachmentRepository.InsertAsync(attachment);
            return result > 0;
        }

        /// <summary>
        /// Sinh mã tra cứu ngẫu nhiên.
        /// </summary>
        private static string GenerateTrackingCode()
        {
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            string randomPart = new Random().Next(1000, 9999).ToString();
            return $"HUE-{datePart}-{randomPart}";
        }
        /// <summary>
        /// Logic xử lý Báo cáo kết quả từ Đơn vị chức năng.
        /// </summary>
        public static async Task<bool> SubmitReportResultAsync(Guid reportId, string status, string resolutionNote)
        {
            var utcNow = DateTime.UtcNow;
            /* Nếu trạng thái là Hoàn thành thì chốt thời gian CompletedAtUtc */
            DateTime? completedAt = status == "HoanThanh" ? utcNow : null;

            var history = new ReportStatusHistory
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                Status = status,
                Note = string.IsNullOrWhiteSpace(resolutionNote) ? "Đơn vị đã cập nhật kết quả xử lý." : $"Kết quả: {resolutionNote}",
                CreatedAtUtc = utcNow
            };

            return await _reportRepository.SubmitResultTransactionAsync(reportId, status, resolutionNote, completedAt, history);
        }
        /// <summary>Lấy danh sách ảnh minh chứng.</summary>
        public static async Task<IEnumerable<ReportAttachment>> GetAttachmentsAsync(Guid reportId)
        {
            return await _attachmentRepository.GetByReportIdAsync(reportId);
        }
        /// <summary>Xóa ảnh đính kèm: Xóa file vật lý trước, xóa Database sau.</summary>
        /// <summary>
        /// Xóa ảnh đính kèm (Chỉ cần xóa trong Database vì dùng cấu trúc BLOB).
        /// </summary>
        public static async Task<bool> DeleteAttachmentAsync(Guid attachmentId)
        {
            try
            {
                int result = await _attachmentRepository.DeleteAsync(attachmentId);
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Lấy danh sách phản ánh của riêng một người dân
        /// </summary>
        public static async Task<IEnumerable<Report>> GetReportsByCustomerIdAsync(Guid customerId)
        {
            return await _reportRepository.GetByCustomerIdAsync(customerId);
        }

    }

    /// <summary>
    /// Model phụ trợ lưu trữ kết quả thống kê Dashboard.
    /// </summary>
    public class DashboardStats
    {
        public int TotalReports { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int PendingApproval { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
        public int NeedsReview { get; set; }
        public int GiaoThong { get; set; }
        public int MoiTruong { get; set; }
        public int HaTang { get; set; }
        public int AnNinh { get; set; }
        public int Khac { get; set; }
    }
}