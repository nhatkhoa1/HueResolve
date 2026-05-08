using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueResolve.Models.Model;

namespace HueResolve.Data.Interfaces
{
    /// <summary>
    /// Interface quản lý file đính kèm (ảnh hiện trường) của phản ánh.
    /// Phục vụ UC01 (gửi phản ánh kèm ảnh) và UC07 (đính kèm ảnh kết quả xử lý).
    /// </summary>
    public interface IReportAttachmentRepository
    {
        /// <summary>
        /// Lưu thông tin một file đính kèm sau khi upload thành công lên server.
        /// Trả về số dòng bị ảnh hưởng.
        /// </summary>
        Task<int> InsertAsync(ReportAttachment attachment);

        /// <summary>
        /// Lấy toàn bộ danh sách file đính kèm của một phản ánh.
        /// Dùng để hiển thị ảnh minh chứng trên trang chi tiết phản ánh.
        /// </summary>
        Task<IEnumerable<ReportAttachment>> GetByReportIdAsync(Guid reportId);

        /// <summary>
        /// Xóa một file đính kèm theo Id.
        /// Dùng khi người dùng hoặc admin xóa ảnh không hợp lệ.
        /// Trả về số dòng bị ảnh hưởng.
        /// </summary>
        Task<int> DeleteAsync(Guid attachmentId);
        Task<ReportAttachment?> GetByIdAsync(Guid id);
    }
}