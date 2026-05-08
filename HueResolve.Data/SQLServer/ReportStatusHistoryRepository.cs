using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;

namespace HueResolve.Data.SQLServer
{
    /// <summary>
    /// Triển khai các thao tác với bảng ReportStatusHistories bằng Dapper.
    /// Đã loại bỏ cột UpdatedByName để tương thích với Database.
    /// </summary>
    public class ReportStatusHistoryRepository : IReportStatusHistoryRepository
    {
        private readonly string _connectionString;

        /// <summary>Khởi tạo với Connection String.</summary>
        public ReportStatusHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>Ghi log thay đổi trạng thái (Timeline).</summary>
        public async Task<int> InsertAsync(ReportStatusHistory history)
        {
            using var connection = new SqlConnection(_connectionString);

            /// Đã xóa bỏ UpdatedByName ra khỏi câu lệnh INSERT
            string sql = @"
                INSERT INTO [dbo].[ReportStatusHistories] (
                    [Id], [ReportId], [Status], [Note], [CreatedAtUtc]
                ) VALUES (
                    @Id, @ReportId, @Status, @Note, @CreatedAtUtc
                );";
            return await connection.ExecuteAsync(sql, history);
        }

        /// <summary>Lấy timeline của phản ánh để hiển thị lên UI.</summary>
        public async Task<IEnumerable<ReportStatusHistory>> GetByReportIdAsync(Guid reportId)
        {
            using var connection = new SqlConnection(_connectionString);

            /// Đã xóa bỏ UpdatedByName ra khỏi câu lệnh SELECT
            string sql = @"
                SELECT [Id], [ReportId], [Status], [Note], [CreatedAtUtc]
                FROM [dbo].[ReportStatusHistories]
                WHERE [ReportId] = @ReportId
                ORDER BY [CreatedAtUtc] DESC;";
            return await connection.QueryAsync<ReportStatusHistory>(sql, new { ReportId = reportId });
        }
    }
}