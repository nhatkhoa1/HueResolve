using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;

namespace HueResolve.Data.SQLServer
{
    /// <summary>
    /// Triển khai truy xuất dữ liệu từ bảng AdministrativeAreas bằng Dapper.
    /// Phục vụ dropdown chọn Phường/Xã khi gửi phản ánh và thống kê bản đồ GIS.
    /// </summary>
    public class AdministrativeAreaRepository : IAdministrativeAreaRepository
    {
        private readonly string _connectionString;

        public AdministrativeAreaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>Lấy toàn bộ danh sách Phường/Xã thuộc Thành phố Huế.</summary>
        public async Task<IEnumerable<AdministrativeArea>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT [Id], [DistrictName], [WardName]
                FROM [dbo].[AdministrativeAreas]
                ORDER BY [DistrictName], [WardName];";
            return await connection.QueryAsync<AdministrativeArea>(sql);
        }
    }
}