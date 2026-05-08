using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;

namespace HueResolve.Data.SQLServer
{
    /// <summary>
    /// Lớp triển khai các thao tác truy xuất và thao tác dữ liệu (Data Access) 
    /// cho nghiệp vụ điều phối và phân công xử lý sự cố.
    /// Sử dụng SQL Transaction để đảm bảo tính toàn vẹn dữ liệu xuyên suốt 3 bảng: Assignments, Reports và ReportStatusHistories.
    /// </summary>
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="AssignmentRepository"/>.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối hợp lệ đến cơ sở dữ liệu SQL Server.</param>
        public AssignmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Thực thi quy trình phân công phức tạp bao gồm 3 bước trong một giao dịch (SQL Transaction) duy nhất:
        /// 1. Lưu thông tin phân công vào bảng Assignments.
        /// 2. Cập nhật ID Đơn vị nhận nhiệm vụ và đổi trạng thái của Phản ánh thành 'DangXuLy' trong bảng Reports.
        /// 3. Ghi vết lịch sử chuyển trạng thái vào bảng Timeline (ReportStatusHistories).
        /// Nếu một trong ba bước thất bại, toàn bộ quá trình sẽ bị Rollback để tránh rác dữ liệu.
        /// </summary>
        /// <param name="assignment">Đối tượng chứa dữ liệu phân công (Đơn vị tiếp nhận, Người giao việc...).</param>
        /// <param name="history">Đối tượng chứa dữ liệu lịch sử để lưu vết trên Timeline.</param>
        /// <returns><c>true</c> nếu toàn bộ giao dịch thành công; ngược lại trả về <c>false</c> và in lỗi ra Console.</returns>
        public async Task<bool> ExecuteAssignmentTransactionAsync(Assignment assignment, ReportStatusHistory history)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                /// 1. Tạo bản ghi phân công trong bảng Assignments
                string sqlInsertAssignment = @"
                    INSERT INTO [dbo].[Assignments] ([Id], [ReportId], [DepartmentId], [AssigneeId], [AssignedAtUtc], [Note])
                    VALUES (@Id, @ReportId, @DepartmentId, @AssigneeId, @AssignedAtUtc, @Note);";
                await connection.ExecuteAsync(sqlInsertAssignment, assignment, transaction);

                /// 2. Cập nhật thực thể phản ánh chính (Gán ID đơn vị và đổi trạng thái sang Đang Xử Lý)
                string sqlUpdateReport = @"
                    UPDATE [dbo].[Reports]
                    SET [AssignedDepartmentId] = @DepartmentId,
                        [Status] = 'DangXuLy',
                        [UpdatedAtUtc] = GETUTCDATE()
                    WHERE [Id] = @ReportId;";
                await connection.ExecuteAsync(sqlUpdateReport, new { assignment.DepartmentId, assignment.ReportId }, transaction);

                /// 3. Ghi vết vào bảng lịch sử (Timeline)
                /// Đã lược bỏ cột UpdatedByName để tương thích hoàn toàn với lược đồ cơ sở dữ liệu hiện tại.
                string sqlInsertHistory = @"
                    INSERT INTO [dbo].[ReportStatusHistories] ([Id], [ReportId], [Status], [Note], [CreatedAtUtc])
                    VALUES (@Id, @ReportId, @Status, @Note, @CreatedAtUtc);";
                await connection.ExecuteAsync(sqlInsertHistory, history, transaction);

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                /// Bắt và in lỗi ra màn hình Console của Visual Studio để phục vụ mục đích Debug
                Console.WriteLine($"[SQL TRANSACTION ERROR]: {ex.Message}");
                transaction.Rollback();
                return false;
            }
        }

        /// <summary>
        /// Thêm mới một bản ghi phân công độc lập vào bảng Assignments.
        /// Hàm này không đi kèm cơ chế Transaction, thường dùng cho các nghiệp vụ cập nhật đơn lẻ.
        /// </summary>
        /// <param name="assignment">Thực thể phân công chứa các trường thông tin hợp lệ.</param>
        /// <returns>Số dòng bị ảnh hưởng bởi truy vấn SQL (thường là 1 nếu thành công).</returns>
        public async Task<int> InsertAsync(Assignment assignment)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO [dbo].[Assignments] ([Id], [ReportId], [DepartmentId], [AssigneeId], [AssignedAtUtc], [Note])
                VALUES (@Id, @ReportId, @DepartmentId, @AssigneeId, @AssignedAtUtc, @Note);";
            return await connection.ExecuteAsync(sql, assignment);
        }

        /// <summary>
        /// Truy xuất thông tin của lượt phân công mới nhất (gần nhất về mặt thời gian) của một phản ánh cụ thể.
        /// Thường được gọi để hiển thị tên "Đơn vị đang thụ lý" trên giao diện Xem chi tiết.
        /// </summary>
        /// <param name="reportId">Mã định danh (Khóa ngoại) của Phản ánh hiện trường.</param>
        /// <returns>Đối tượng <see cref="Assignment"/> mới nhất nếu có; ngược lại trả về <c>null</c>.</returns>
        public async Task<Assignment?> GetLatestByReportIdAsync(Guid reportId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT TOP 1 [Id], [ReportId], [DepartmentId], [AssigneeId], [AssignedAtUtc], [Note]
                FROM [dbo].[Assignments]
                WHERE [ReportId] = @ReportId
                ORDER BY [AssignedAtUtc] DESC;";
            return await connection.QuerySingleOrDefaultAsync<Assignment>(sql, new { ReportId = reportId });
        }
    }
}