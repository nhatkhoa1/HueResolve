using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Interface quản lý danh mục Phường/Xã phục vụ nhập liệu và thống kê bản đồ.
    /// </summary>
    public interface IAdministrativeAreaRepository
    {
        /// <summary>Lấy danh sách phường xã thuộc Thành phố Huế.</summary>
        Task<IEnumerable<AdministrativeArea>> GetAllAsync();
    }
}