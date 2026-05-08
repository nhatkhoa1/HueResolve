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
    /// Lớp triển khai các thao tác truy xuất dữ liệu (Data Access) cho bảng [Categories] (Danh mục / Lĩnh vực sự cố).
    /// Sử dụng thư viện Dapper để ánh xạ dữ liệu trực tiếp từ SQL Server nhằm tối ưu hóa hiệu năng.
    /// Đã cấu hình các câu truy vấn SELECT để sắp xếp danh sách theo thứ tự giảm dần (ORDER BY [Id] DESC).
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="CategoryRepository"/>.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối hợp lệ đến cơ sở dữ liệu SQL Server.</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy xuất toàn bộ danh sách các danh mục hiện có trong cơ sở dữ liệu.
        /// Thường được sử dụng để đổ dữ liệu vào Dropdown list khi người dùng gửi phản ánh.
        /// Danh sách được sắp xếp mới nhất lên đầu dựa vào [Id] DESC.
        /// </summary>
        /// <returns>Tập hợp <see cref="IEnumerable{Category}"/> chứa toàn bộ danh mục.</returns>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT [Id], [Code], [Name], [Description] FROM [dbo].[Categories] ORDER BY [Id] DESC;";
            return await connection.QueryAsync<Category>(sql);
        }

        /// <summary>
        /// Tra cứu thông tin chi tiết của một danh mục cụ thể dựa trên khóa chính (Id).
        /// Hỗ trợ cho các tác vụ như Xem chi tiết hoặc Sửa thông tin danh mục.
        /// </summary>
        /// <param name="id">Mã định danh của danh mục cần tìm.</param>
        /// <returns>Đối tượng <see cref="Category"/> nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
        public async Task<Category?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT [Id], [Code], [Name], [Description] FROM [dbo].[Categories] WHERE [Id] = @Id;";
            return await connection.QuerySingleOrDefaultAsync<Category>(sql, new { Id = id });
        }

        /// <summary>
        /// Truy xuất danh sách danh mục với cơ chế phân trang và lọc theo từ khóa tìm kiếm.
        /// Sử dụng kỹ thuật OFFSET-FETCH của SQL Server để lấy đúng số lượng bản ghi cần thiết.
        /// </summary>
        /// <param name="page">Số thứ tự của trang hiện tại (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số lượng danh mục hiển thị tối đa trên một trang.</param>
        /// <param name="search">Từ khóa tìm kiếm (theo Mã hoặc Tên danh mục). Chấp nhận giá trị null.</param>
        /// <returns>Một Tuple chứa: Danh sách dữ liệu của trang (<c>Data</c>) và Tổng số bản ghi thỏa điều kiện (<c>TotalCount</c>).</returns>
        public async Task<(IEnumerable<Category> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
        {
            using var connection = new SqlConnection(_connectionString);
            int offset = (page - 1) * pageSize;
            string searchPattern = string.IsNullOrWhiteSpace(search) ? null! : $"%{search}%";

            string sqlData = @"
                SELECT [Id], [Code], [Name], [Description]
                FROM [dbo].[Categories]
                WHERE (@Search IS NULL OR [Name] LIKE @Search OR [Code] LIKE @Search)
                ORDER BY [Id] ASC 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string sqlCount = @"
                SELECT COUNT(*) FROM [dbo].[Categories]
                WHERE (@Search IS NULL OR [Name] LIKE @Search OR [Code] LIKE @Search);";

            var data = await connection.QueryAsync<Category>(sqlData, new { Search = searchPattern, Offset = offset, PageSize = pageSize });
            int totalCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { Search = searchPattern });

            return (data, totalCount);
        }

        /// <summary>
        /// Thêm mới một danh mục (lĩnh vực sự cố) vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="category">Thực thể <see cref="Category"/> chứa các thông tin cần lưu (Mã, Tên, Mô tả).</param>
        /// <returns>Số dòng bị tác động trong CSDL (thường là 1 nếu chèn thành công).</returns>
        public async Task<int> InsertAsync(Category category)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "INSERT INTO [dbo].[Categories] ([Code], [Name], [Description]) VALUES (@Code, @Name, @Description);";
            return await connection.ExecuteAsync(sql, category);
        }

        /// <summary>
        /// Cập nhật thông tin của một danh mục đã tồn tại trong hệ thống.
        /// </summary>
        /// <param name="category">Thực thể <see cref="Category"/> chứa ID cần sửa và các thông tin đã cập nhật.</param>
        /// <returns>Số dòng bị tác động trong CSDL (1 nếu cập nhật thành công).</returns>
        public async Task<int> UpdateAsync(Category category)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "UPDATE [dbo].[Categories] SET [Code] = @Code, [Name] = @Name, [Description] = @Description WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, category);
        }

        /// <summary>
        /// Xóa vĩnh viễn một danh mục khỏi hệ thống dựa trên khóa chính.
        /// Chú ý: SQL Server sẽ quăng ngoại lệ (Exception) nếu danh mục này đang được tham chiếu bởi các Phản ánh (Foreign Key Constraint).
        /// </summary>
        /// <param name="id">Mã định danh của danh mục cần xóa.</param>
        /// <returns>Số dòng bị xóa (1 nếu xóa thành công).</returns>
        public async Task<int> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM [dbo].[Categories] WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}