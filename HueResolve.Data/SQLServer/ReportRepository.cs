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
    /// Lớp thực thi (Implementation) chuyên trách tương tác trực tiếp với cơ sở dữ liệu SQL Server
    /// thông qua Dapper ORM cho thực thể <see cref="Report"/> (Phản ánh hiện trường).
    /// Tuân thủ hợp đồng giao tiếp từ <see cref="IReportRepository"/>.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="ReportRepository"/>.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến cơ sở dữ liệu SQL Server (được cung cấp từ cấu hình ứng dụng).</param>
        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn và trả về chi tiết một phản ánh hiện trường cụ thể dựa trên định danh duy nhất (Id).
        /// </summary>
        /// <param name="id">Khóa chính của bản ghi phản ánh kiểu <see cref="Guid"/>.</param>
        /// <returns>Đối tượng <see cref="Report"/> chứa toàn bộ thông tin chi tiết; hoặc <c>null</c> nếu không tìm thấy.</returns>
        public async Task<Report?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    [Id], [TrackingCode], [Title], [Description], [ReporterName], 
                    [ReporterPhone], [AddressText], [WardName], [DistrictName], 
                    [Latitude], [Longitude], [CategoryId], [AdministrativeAreaId], 
                    [AiConfidence], [ClassificationState], [NeedsReview], [Status], 
                    [ResolutionNote], [CompletedAtUtc], [CreatedAtUtc], [UpdatedAtUtc], 
                    [CustomerId], [AssignedDepartmentId], [AdminFeedback]
                FROM [dbo].[Reports] 
                WHERE [Id] = @Id;";
            return await connection.QuerySingleOrDefaultAsync<Report>(sql, new { Id = id });
        }

        /// <summary>
        /// Tra cứu thông tin phản ánh thông qua Mã tra cứu công khai (Tracking Code).
        /// Thường được sử dụng ở phân hệ dành cho Người dân để xem tiến độ sự cố.
        /// </summary>
        /// <param name="trackingCode">Chuỗi mã tra cứu duy nhất của phản ánh (Ví dụ: HUE-20240101-1234).</param>
        /// <returns>Đối tượng <see cref="Report"/> nếu mã tra cứu hợp lệ; ngược lại trả về <c>null</c>.</returns>
        public async Task<Report?> GetByTrackingCodeAsync(string trackingCode)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM [dbo].[Reports] WHERE [TrackingCode] = @Code;";
            return await connection.QuerySingleOrDefaultAsync<Report>(sql, new { Code = trackingCode });
        }

        /// <summary>
        /// Truy xuất toàn bộ danh sách phản ánh từ cơ sở dữ liệu, hỗ trợ bộ lọc đa điều kiện.
        /// Hàm này không sử dụng phân trang, thường dùng để tính toán thống kê tổng quan (Dashboard Stats).
        /// </summary>
        /// <param name="status">Lọc theo trạng thái hiện tại (TiepNhan, DangXuLy...). Truyền <c>null</c> để lấy tất cả.</param>
        /// <param name="categoryId">Lọc theo ID lĩnh vực sự cố (Giao thông, Môi trường...). Truyền <c>null</c> để bỏ qua.</param>
        /// <param name="search">Từ khóa tìm kiếm linh hoạt trên Tiêu đề hoặc Mã tra cứu.</param>
        /// <returns>Một tập hợp <see cref="IEnumerable{Report}"/> chứa các phản ánh thỏa mãn điều kiện.</returns>
        public async Task<IEnumerable<Report>> GetAllAsync(string? status = null, int? categoryId = null, string? search = null)
        {
            using var connection = new SqlConnection(_connectionString);
            string? searchPattern = string.IsNullOrEmpty(search) ? null : $"%{search}%";
            string sql = @"
                SELECT * FROM [dbo].[Reports] 
                WHERE (@Status IS NULL OR [Status] = @Status)
                  AND (@CategoryId IS NULL OR [CategoryId] = @CategoryId)
                  AND (@Search IS NULL OR [Title] LIKE @Search OR [TrackingCode] LIKE @Search);";
            return await connection.QueryAsync<Report>(sql, new { Status = status, CategoryId = categoryId, Search = searchPattern });
        }

        /// <summary>
        /// Truy xuất danh sách phản ánh với cơ chế phân trang (Pagination) và các bộ lọc tìm kiếm.
        /// Sử dụng kỹ thuật OFFSET-FETCH của SQL Server để tối ưu hiệu năng đọc dữ liệu khối lượng lớn.
        /// </summary>
        /// <param name="page">Chỉ số trang hiện tại cần lấy (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số lượng bản ghi tối đa hiển thị trên mỗi trang.</param>
        /// <param name="status">Lọc theo trạng thái xử lý (Tùy chọn).</param>
        /// <param name="categoryId">Lọc theo lĩnh vực phản ánh (Tùy chọn).</param>
        /// <param name="search">Từ khóa tìm kiếm trong Tiêu đề hoặc Mã tra cứu (Tùy chọn).</param>
        /// <returns>Một Tuple chứa: tập hợp dữ liệu phản ánh của trang hiện tại (<c>Data</c>) và tổng số bản ghi thỏa điều kiện (<c>TotalCount</c>).</returns>
        public async Task<(IEnumerable<Report> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? status, int? categoryId, string? search)
        {
            using var connection = new SqlConnection(_connectionString);
            int offset = (page - 1) * pageSize;
            string? searchPattern = string.IsNullOrEmpty(search) ? null : $"%{search}%";

            string sqlData = @"
                SELECT 
                    [Id], [TrackingCode], [Title], [Description], [ReporterName], 
                    [ReporterPhone], [AddressText], [WardName], [DistrictName], 
                    [Latitude], [Longitude], [CategoryId], [AdministrativeAreaId], 
                    [AiConfidence], [ClassificationState], [NeedsReview], [Status], 
                    [ResolutionNote], [CompletedAtUtc], [CreatedAtUtc], [UpdatedAtUtc], 
                    [CustomerId], [AssignedDepartmentId], [AdminFeedback]
                FROM [dbo].[Reports]
                WHERE (@Status IS NULL OR [Status] = @Status)
                  AND (@CategoryId IS NULL OR [CategoryId] = @CategoryId)
                  AND (@Search IS NULL OR [Title] LIKE @Search OR [TrackingCode] LIKE @Search)
                ORDER BY [CreatedAtUtc] DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string sqlCount = @"
                SELECT COUNT(*) FROM [dbo].[Reports]
                WHERE (@Status IS NULL OR [Status] = @Status)
                  AND (@CategoryId IS NULL OR [CategoryId] = @CategoryId)
                  AND (@Search IS NULL OR [Title] LIKE @Search OR [TrackingCode] LIKE @Search);";

            var parameters = new
            {
                Status = status,
                CategoryId = categoryId,
                Search = searchPattern,
                Offset = offset,
                PageSize = pageSize
            };

            var data = await connection.QueryAsync<Report>(sqlData, parameters);
            int totalCount = await connection.ExecuteScalarAsync<int>(sqlCount, parameters);

            return (data, totalCount);
        }

        /// <summary>
        /// Truy xuất danh sách các phản ánh có chứa thông tin tọa độ địa lý hợp lệ để hiển thị trên Bản đồ điểm nóng (GIS).
        /// Đã tối ưu hóa lệnh SELECT chỉ lấy các trường thật sự cần thiết (Id, Tọa độ, Tiêu đề, Trạng thái, Mã PA, Địa chỉ).
        /// </summary>
        /// <param name="categoryId">Lọc dữ liệu bản đồ theo chuyên mục sự cố nhất định.</param>
        /// <param name="fromDate">Mốc thời gian bắt đầu quét dữ liệu phản ánh (Tùy chọn).</param>
        /// <param name="toDate">Mốc thời gian kết thúc quét dữ liệu phản ánh (Tùy chọn).</param>
        /// <returns>Tập hợp các phản ánh đáp ứng yêu cầu render marker lên Web GIS.</returns>
        public async Task<IEnumerable<Report>> GetForMapAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    [Id], [TrackingCode], [Latitude], [Longitude], [Title], 
                    [Status], [CategoryId], [AddressText]
                FROM [dbo].[Reports] 
                WHERE [Latitude] IS NOT NULL
                  AND (@CategoryId IS NULL OR [CategoryId] = @CategoryId)
                  AND (@FromDate IS NULL OR [CreatedAtUtc] >= @FromDate)
                  AND (@ToDate IS NULL OR [CreatedAtUtc] <= @ToDate);";
            return await connection.QueryAsync<Report>(sql, new { CategoryId = categoryId, FromDate = fromDate, ToDate = toDate });
        }

        /// <summary>
        /// Thêm mới một bản ghi phản ánh hiện trường vào cơ sở dữ liệu.
        /// Thường được gọi khi Người dân hoàn tất gửi biểu mẫu báo cáo sự cố qua cổng thông tin.
        /// </summary>
        /// <param name="report">Đối tượng <see cref="Report"/> chứa toàn bộ dữ liệu người dùng cung cấp.</param>
        /// <returns>Số dòng bị ảnh hưởng bởi lệnh thực thi (thường là 1 nếu thành công).</returns>
        public async Task<int> InsertAsync(Report report)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO [dbo].[Reports] (
                    [Id], [TrackingCode], [Title], [Description], [ReporterName], 
                    [ReporterPhone], [AddressText], [WardName], [DistrictName], 
                    [Latitude], [Longitude], [CategoryId], [AdministrativeAreaId], 
                    [AiConfidence], [ClassificationState], [NeedsReview], [Status], 
                    [CreatedAtUtc], [UpdatedAtUtc], [CustomerId]
                ) VALUES (
                    @Id, @TrackingCode, @Title, @Description, @ReporterName, 
                    @ReporterPhone, @AddressText, @WardName, @DistrictName, 
                    @Latitude, @Longitude, @CategoryId, @AdministrativeAreaId, 
                    @AiConfidence, @ClassificationState, @NeedsReview, @Status, 
                    @CreatedAtUtc, @UpdatedAtUtc, @CustomerId
                );";
            return await connection.ExecuteAsync(sql, report);
        }

        /// <summary>
        /// Ghi nhận kết quả dự đoán và phân loại danh mục tự động từ mô hình Trí tuệ nhân tạo (AI PhoBERT).
        /// Cập nhật trường cập nhật thời gian (UpdatedAtUtc) song song để hệ thống ghi vết.
        /// </summary>
        /// <param name="reportId">Định danh của phản ánh vừa được phân tích.</param>
        /// <param name="categoryId">ID của Lĩnh vực mà AI đề xuất (Có thể Null nếu AI không nhận diện được).</param>
        /// <param name="classificationState">Trạng thái quá trình phân tích (Success, LowConfidence, Failed).</param>
        /// <param name="confidence">Điểm số xác suất đánh giá mức độ chắc chắn của AI (từ 0.0 đến 1.0).</param>
        /// <param name="needsReview">Cờ hiệu đánh dấu cần Cán bộ IOC kiểm duyệt thủ công nếu độ tin cậy thấp.</param>
        /// <returns>Số dòng bị ảnh hưởng (1 nếu thành công).</returns>
        public async Task<int> UpdateAiClassificationAsync(Guid reportId, int? categoryId, string classificationState, double confidence, bool needsReview)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Reports] 
                SET [CategoryId] = @CatId, 
                    [ClassificationState] = @State, 
                    [AiConfidence] = @Conf, 
                    [NeedsReview] = @Review,
                    [UpdatedAtUtc] = GETUTCDATE()
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { CatId = categoryId, State = classificationState, Conf = confidence, Review = needsReview, Id = reportId });
        }

        /// <summary>
        /// Cập nhật thay đổi trạng thái tổng thể của quá trình xử lý sự cố.
        /// Hỗ trợ ghi lại ghi chú giải quyết và đánh dấu thời điểm kết thúc quy trình.
        /// </summary>
        /// <param name="reportId">ID phản ánh cần cập nhật.</param>
        /// <param name="newStatus">Chuỗi ký hiệu trạng thái mới (VD: HoanThanh, TuChoi).</param>
        /// <param name="resolutionNote">Ghi chú chi tiết về phương án đã giải quyết hoặc lý do từ chối (Tùy chọn).</param>
        /// <param name="completedAtUtc">Lưu vết thời gian hoàn thành chuẩn UTC nếu trạng thái là HoanThanh.</param>
        /// <returns>Số dòng bị ảnh hưởng trong DB.</returns>
        public async Task<int> UpdateStatusAsync(Guid reportId, string newStatus, string? resolutionNote, DateTime? completedAtUtc)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Reports]
                SET [Status] = @Status,
                    [ResolutionNote] = @Note,
                    [CompletedAtUtc] = @CompletedAt,
                    [UpdatedAtUtc] = GETUTCDATE()
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Status = newStatus, Note = resolutionNote, CompletedAt = completedAtUtc, Id = reportId });
        }

        /// <summary>
        /// Cập nhật định danh của Đơn vị / Phòng ban chức năng chịu trách nhiệm tiếp nhận và xử lý sự cố thực địa.
        /// </summary>
        /// <param name="reportId">ID của phản ánh.</param>
        /// <param name="departmentId">Khóa ngoại tham chiếu đến bảng Đơn vị xử lý.</param>
        /// <returns>Số dòng được cập nhật thành công.</returns>
        public async Task<int> UpdateAssignedDepartmentAsync(Guid reportId, int departmentId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Reports] 
                SET [AssignedDepartmentId] = @DeptId, 
                    [UpdatedAtUtc] = GETUTCDATE() 
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { DeptId = departmentId, Id = reportId });
        }

        /// <summary>
        /// Ghi nhận đoạn văn bản thông điệp phản hồi từ Cán bộ IOC hoặc Admin.
        /// Nội dung này sẽ được hiển thị công khai trên ứng dụng Hue-S để Người dân đọc được tiến độ chi tiết.
        /// </summary>
        /// <param name="reportId">ID của sự cố hiện trường.</param>
        /// <param name="feedback">Nội dung văn bản phản hồi.</param>
        /// <returns>Số dòng được cập nhật.</returns>
        public async Task<int> UpdateAdminFeedbackAsync(Guid reportId, string feedback)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Reports] 
                SET [AdminFeedback] = @Feedback, 
                    [UpdatedAtUtc] = GETUTCDATE() 
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Feedback = feedback, Id = reportId });
        }


        /// <summary>
        /// Cập nhật ĐỒNG THỜI Đơn vị được giao xử lý và Trạng thái phản ánh mới trong cùng một lượt.
        /// </summary>
        /// <param name="reportId">ID phản ánh mục tiêu.</param>
        /// <param name="departmentId">ID Đơn vị tiếp nhận mới.</param>
        /// <param name="status">Trạng thái mới cần chuyển đổi (VD: DangXuLy).</param>
        /// <returns>Số bản ghi chịu ảnh hưởng.</returns>
        public async Task<int> UpdateAssignmentAsync(Guid reportId, int departmentId, string status)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Reports] 
                SET [AssignedDepartmentId] = @DeptId, 
                    [Status] = @Status, 
                    [UpdatedAtUtc] = GETUTCDATE() 
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { DeptId = departmentId, Status = status, Id = reportId });
        }
        public async Task<bool> SubmitResultTransactionAsync(Guid reportId, string status, string resolutionNote, DateTime? completedAt, ReportStatusHistory history)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                /* 1. Cập nhật bảng Reports */
                string sqlUpdateReport = @"
                    UPDATE [dbo].[Reports]
                    SET [Status] = @Status,
                        [ResolutionNote] = @ResolutionNote,
                        [CompletedAtUtc] = @CompletedAtUtc,
                        [UpdatedAtUtc] = GETUTCDATE()
                    WHERE [Id] = @ReportId;";

                await connection.ExecuteAsync(sqlUpdateReport, new
                {
                    Status = status,
                    ResolutionNote = resolutionNote,
                    CompletedAtUtc = completedAt,
                    ReportId = reportId
                }, transaction);

                /* 2. Ghi log lịch sử */
                string sqlInsertHistory = @"
                    INSERT INTO [dbo].[ReportStatusHistories] ([Id], [ReportId], [Status], [Note], [CreatedAtUtc])
                    VALUES (@Id, @ReportId, @Status, @Note, @CreatedAtUtc);";
                await connection.ExecuteAsync(sqlInsertHistory, history, transaction);

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }
        /// <summary>
        /// Lấy danh sách phản ánh theo ID của người dân (Sắp xếp mới nhất lên đầu)
        /// </summary>
        public async Task<IEnumerable<Report>> GetByCustomerIdAsync(Guid customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM [dbo].[Reports] WHERE CustomerId = @CustomerId ORDER BY CreatedAtUtc DESC";
            return await connection.QueryAsync<Report>(sql, new { CustomerId = customerId });
        }
    }
}