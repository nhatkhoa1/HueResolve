using System;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Dịch vụ tĩnh điều phối nghiệp vụ phân công xử lý phản ánh hiện trường.
    /// </summary>
    public static class AssignmentService
    {
        private static IAssignmentRepository _assignmentRepository = default!;

        /// <summary>Khởi tạo dịch vụ với tham số cấu hình.</summary>
        public static void Initialize(string connectionString)
        {
            _assignmentRepository = new AssignmentRepository(connectionString);
        }

        /// <summary>Nghiệp vụ cốt lõi: Phân công một phản ánh cho Đơn vị chức năng.</summary>
        public static async Task<bool> AssignReportToDepartmentAsync(Guid reportId, int departmentId, Guid assignedByUserId, string note, string adminName)
        {
            var utcNow = DateTime.UtcNow;

            /// Tạo đối tượng phân công với tên thuộc tính AssigneeId đã được đồng bộ
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                DepartmentId = departmentId,
                AssigneeId = assignedByUserId,
                AssignedAtUtc = utcNow,
                Note = note
            };

            /// Tạo đối tượng lịch sử
            var history = new ReportStatusHistory
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                Status = "DangXuLy",
                Note = string.IsNullOrWhiteSpace(note) ? "Đã chuyển đơn vị xử lý." : $"Đã điều phối đơn vị. Nội dung: {note}",
                UpdatedByName = adminName,
                CreatedAtUtc = utcNow
            };

            return await _assignmentRepository.ExecuteAssignmentTransactionAsync(assignment, history);
        }

        /// <summary>Lấy phân công mới nhất.</summary>
        public static async Task<Assignment?> GetLatestAssignmentAsync(Guid reportId)
        {
            return await _assignmentRepository.GetLatestByReportIdAsync(reportId);
        }
    }
}