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
    /// Lớp triển khai thực tế các truy vấn SQL Server cho thực thể User bằng Dapper.
    /// Sử dụng RAW SQL để tối ưu hiệu năng và kiểm soát chặt chẽ các cột dữ liệu.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo Repository với chuỗi kết nối SQL Server.
        /// </summary>
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn người dùng theo Username (phục vụ đăng nhập).
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT [Id], [FullName], [Username], [PasswordHash], [RoleId], [CreatedAtUtc], [PhoneNumber], [AddressText], [DepartmentId], [IsActive], [AvatarPath]
                FROM [dbo].[Users]
                WHERE [Username] = @Username;";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
        }

        /// <summary>
        /// Lấy chi tiết tài khoản theo Id.
        /// </summary>
        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT [Id], [FullName], [Username], [PasswordHash], [RoleId], [CreatedAtUtc], [PhoneNumber], [AddressText], [DepartmentId], [IsActive], [AvatarPath]
                FROM [dbo].[Users]
                WHERE [Id] = @Id;";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        /// <summary>
        /// Thực thi truy vấn phân trang phía Server (Server-side Pagination).
        /// </summary>
        public async Task<(IEnumerable<User> Data, int TotalCount)> GetPagedAsync(int page, int pageSize, int? roleId, string? search)
        {
            using var connection = new SqlConnection(_connectionString);
            int offset = (page - 1) * pageSize;
            string searchPattern = string.IsNullOrWhiteSpace(search) ? null! : $"%{search}%";

            string sqlData = @"
                SELECT [Id], [FullName], [Username], [RoleId], [CreatedAtUtc], [PhoneNumber], [IsActive], [AvatarPath]
                FROM [dbo].[Users]
                WHERE (@RoleId IS NULL OR [RoleId] = @RoleId)
                AND (@Search IS NULL OR [FullName] LIKE @Search OR [Username] LIKE @Search)
                ORDER BY [CreatedAtUtc] DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string sqlCount = @"
                SELECT COUNT(*) FROM [dbo].[Users]
                WHERE (@RoleId IS NULL OR [RoleId] = @RoleId)
                AND (@Search IS NULL OR [FullName] LIKE @Search OR [Username] LIKE @Search);";

            var data = await connection.QueryAsync<User>(sqlData, new { RoleId = roleId, Search = searchPattern, Offset = offset, PageSize = pageSize });
            int totalCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { RoleId = roleId, Search = searchPattern });

            return (data, totalCount);
        }

        /// <summary>
        /// Thêm mới tài khoản cán bộ/người dân.
        /// </summary>
        public async Task<int> InsertAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO [dbo].[Users] (
                    [Id], [FullName], [Username], [PasswordHash], [RoleId],
                    [CreatedAtUtc], [PhoneNumber], [AddressText], [DepartmentId], [IsActive], [AvatarPath]
                ) VALUES (
                    @Id, @FullName, @Username, @PasswordHash, @RoleId,
                    @CreatedAtUtc, @PhoneNumber, @AddressText, @DepartmentId, @IsActive, @AvatarPath
                );";
            return await connection.ExecuteAsync(sql, user);
        }

        /// <summary>
        /// Vô hiệu hóa hoặc kích hoạt lại tài khoản.
        /// </summary>
        public async Task<int> UpdateIsActiveAsync(Guid userId, bool isActive)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE [dbo].[Users] SET [IsActive] = @IsActive WHERE [Id] = @UserId;";
            return await connection.ExecuteAsync(sql, new { UserId = userId, IsActive = isActive });
        }

        /// <summary>
        /// Cập nhật mật khẩu tài khoản (MD5 hash).
        /// </summary>
        public async Task<int> UpdatePasswordAsync(Guid userId, string newPasswordHash)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE [dbo].[Users] SET [PasswordHash] = @PasswordHash WHERE [Id] = @UserId;";
            return await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = newPasswordHash });
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân của người dùng (FullName, PhoneNumber, AddressText).
        /// </summary>
        public async Task<int> UpdateAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE [dbo].[Users]
                SET [FullName] = @FullName, [PhoneNumber] = @PhoneNumber, [AddressText] = @AddressText, [AvatarPath] = @AvatarPath
                WHERE [Id] = @Id;";
            return await connection.ExecuteAsync(sql, user);
        }
    }
}