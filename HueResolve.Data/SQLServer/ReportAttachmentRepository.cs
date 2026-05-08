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
    /// Triển khai các thao tác với bảng ReportAttachments bằng Dapper.
    /// Quản lý vòng đời file đính kèm: lưu metadata sau upload, truy xuất và xóa.
    /// </summary>
    public class ReportAttachmentRepository : IReportAttachmentRepository
    {
        private readonly string _connectionString;

        public ReportAttachmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lưu thông tin metadata của file sau khi upload thành công lên server (UC01, UC07).
        /// Lưu ý: Chỉ lưu metadata — việc lưu file vật lý do Business Layer xử lý trước.
        /// </summary>
        public async Task<int> InsertAsync(ReportAttachment attachment)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO [dbo].[ReportAttachments] (
                    [Id], [ReportId], [OriginalFileName], [StoredFileName],
                    [RelativePath], [ContentType], [CreatedAtUtc], [FileData]
                ) VALUES (
                    @Id, @ReportId, @OriginalFileName, @StoredFileName,
                    @RelativePath, @ContentType, @CreatedAtUtc, @FileData
                );";
            return await connection.ExecuteAsync(sql, attachment);
        }

        /// <summary>
        /// Lấy toàn bộ file đính kèm của một phản ánh để hiển thị ảnh minh chứng.
        /// Sắp xếp theo thời gian upload tăng dần.
        /// </summary>
        public async Task<IEnumerable<ReportAttachment>> GetByReportIdAsync(Guid reportId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM [dbo].[ReportAttachments] WHERE [ReportId] = @ReportId ORDER BY [CreatedAtUtc] ASC;";
            return await connection.QueryAsync<ReportAttachment>(sql, new { ReportId = reportId });
        }

        /// <summary>
        /// Xóa metadata file đính kèm theo Id.
        /// Lưu ý: Xóa file vật lý khỏi disk do Business Layer xử lý sau khi gọi method này.
        /// </summary>
        public async Task<int> DeleteAsync(Guid attachmentId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                DELETE FROM [dbo].[ReportAttachments]
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = attachmentId });
        }
        /// <summary>
        /// Lấy thông tin chi tiết của một file đính kèm dựa trên ID.
        /// Cần thiết để lấy RelativePath phục vụ việc xóa file vật lý trên server.
        /// </summary>
        public async Task<ReportAttachment?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    [Id], [ReportId], [OriginalFileName], [StoredFileName], 
                    [RelativePath], [ContentType], [CreatedAtUtc]
                FROM [dbo].[ReportAttachments] 
                WHERE [Id] = @Id;";
            return await connection.QuerySingleOrDefaultAsync<ReportAttachment>(sql, new { Id = id });
        }
    }
}