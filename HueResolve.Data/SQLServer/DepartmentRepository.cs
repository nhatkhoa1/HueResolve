using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;

namespace HueResolve.Data.SQLServer
{
    /// <summary>
    /// Lớp triển khai (Implementation) các thao tác truy xuất dữ liệu từ cơ sở dữ liệu SQL Server 
    /// cho bảng [Departments] (Đơn vị xử lý / Phòng ban).
    /// Giao tiếp với DB thông qua Micro-ORM Dapper để tối ưu hóa hiệu năng (RAW SQL).
    /// </summary>
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="DepartmentRepository"/> với chuỗi kết nối.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối hợp lệ tới SQL Server (cấu hình trong appsettings).</param>
        public DepartmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một Đơn vị dựa trên khóa chính (Id).
        /// </summary>
        /// <param name="id">Mã định danh của Đơn vị.</param>
        /// <returns>Đối tượng <see cref="Department"/> chứa toàn bộ trường thông tin; hoặc <c>null</c> nếu không tồn tại.</returns>
        public async Task<Department?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT [Id], [Name], [Description], [Email], [PhoneNumber], [IsActive], [Type]
                FROM [dbo].[Departments]
                WHERE [Id] = @Id;";
            return await connection.QuerySingleOrDefaultAsync<Department>(sql, new { Id = id });
        }

        /// <summary>
        /// Truy xuất danh sách toàn bộ các Đơn vị đang trong trạng thái Hoạt động (<c>IsActive = 1</c>).
        /// Sắp xếp theo thứ tự chữ cái của Tên đơn vị.
        /// Thường dùng để đổ dữ liệu (populate) vào Dropdown phân công trên giao diện Admin.
        /// </summary>
        /// <returns>Tập hợp <see cref="IEnumerable{Department}"/> chứa các Đơn vị hợp lệ.</returns>
        public async Task<IEnumerable<Department>> GetAllActiveAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT [Id], [Name], [Description], [Email], [PhoneNumber], [IsActive], [Type]
                FROM [dbo].[Departments]
                WHERE [IsActive] = 1
                ORDER BY [Name];";
            return await connection.QueryAsync<Department>(sql);
        }

        /// <summary>
        /// Lấy danh sách các Đơn vị có hỗ trợ phân trang (Pagination) và tìm kiếm (Search).
        /// Sử dụng kỹ thuật OFFSET-FETCH để tối ưu truy vấn trên SQL Server.
        /// </summary>
        /// <param name="page">Số thứ tự trang hiện tại (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số lượng bản ghi hiển thị trên mỗi trang.</param>
        /// <param name="search">Từ khóa tìm kiếm (so khớp linh hoạt với Tên, Email hoặc Số điện thoại). Null nếu không tìm kiếm.</param>
        /// <returns>Một Tuple gồm hai giá trị: Danh sách Đơn vị của trang hiện tại (<c>Data</c>) và Tổng số đơn vị thỏa điều kiện tìm kiếm (<c>TotalCount</c>).</returns>
        public async Task<(IEnumerable<Department> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
        {
            using var connection = new SqlConnection(_connectionString);
            int offset = (page - 1) * pageSize;
            string searchPattern = string.IsNullOrWhiteSpace(search) ? null! : $"%{search}%";

            string sqlData = @"
                SELECT [Id], [Name], [Description], [Email], [PhoneNumber], [IsActive], [Type]
                FROM [dbo].[Departments]
                WHERE (@Search IS NULL OR [Name] LIKE @Search OR [Email] LIKE @Search OR [PhoneNumber] LIKE @Search)
                ORDER BY [Name] ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string sqlCount = @"
                SELECT COUNT(*) FROM [dbo].[Departments]
                WHERE (@Search IS NULL OR [Name] LIKE @Search OR [Email] LIKE @Search OR [PhoneNumber] LIKE @Search);";

            var data = await connection.QueryAsync<Department>(sqlData, new { Search = searchPattern, Offset = offset, PageSize = pageSize });
            int totalCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { Search = searchPattern });

            return (data, totalCount);
        }

        /// <summary>
        /// Thêm mới một Đơn vị vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="department">Đối tượng <see cref="Department"/> chứa thông tin cần lưu.</param>
        /// <returns>Số dòng bị tác động (thường là 1 nếu chèn thành công).</returns>
        public async Task<int> InsertAsync(Department department)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO [dbo].[Departments] ([Name], [Description], [Email], [PhoneNumber], [IsActive], [Type])
                VALUES (@Name, @Description, @Email, @PhoneNumber, @IsActive, @Type);";
            return await connection.ExecuteAsync(sql, department);
        }

        /// <summary>
        /// Cập nhật thông tin chung (Tên, Mô tả, Liên hệ, Loại hình) của một Đơn vị đã tồn tại.
        /// Chú ý: Hàm này KHÔNG cập nhật trường trạng thái <c>IsActive</c>.
        /// </summary>
        /// <param name="department">Đối tượng <see cref="Department"/> chứa thông tin mới.</param>
        /// <returns>Số dòng được cập nhật thành công trong CSDL.</returns>
        public async Task<int> UpdateAsync(Department department)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Departments]
                SET [Name] = @Name,
                    [Description] = @Description,
                    [Email] = @Email,
                    [PhoneNumber] = @PhoneNumber,
                    [Type] = @Type
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, department);
        }

        /// <summary>
        /// Thay đổi trạng thái Khóa / Mở khóa của một Đơn vị bằng cách cập nhật cờ <c>IsActive</c>.
        /// </summary>
        /// <param name="id">Khóa chính của Đơn vị.</param>
        /// <param name="isActive">Trạng thái mới: <c>true</c> (Hoạt động) hoặc <c>false</c> (Khóa).</param>
        /// <returns>Số dòng bị tác động.</returns>
        public async Task<int> UpdateIsActiveAsync(int id, bool isActive)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE [dbo].[Departments] SET [IsActive] = @IsActive WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        }

        /// <summary>
        /// Xóa vĩnh viễn (Hard Delete) bản ghi của một Đơn vị khỏi cơ sở dữ liệu.
        /// Lưu ý: Cần xử lý ràng buộc khóa ngoại (Foreign Key Constraints) trước khi xóa để tránh lỗi SQL.
        /// </summary>
        /// <param name="id">Khóa chính của Đơn vị cần xóa.</param>
        /// <returns>Số bản ghi đã bị xóa khỏi hệ thống.</returns>
        public async Task<int> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM [dbo].[Departments] WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}