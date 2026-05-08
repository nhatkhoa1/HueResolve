namespace HueResolve.Business.Services
{
    /// <summary>
    /// Lớp tĩnh quản lý thông tin cấu hình dùng chung cho toàn bộ Business Layer.
    /// Đóng vai trò là trung tâm lưu trữ chuỗi kết nối cơ sở dữ liệu.
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = string.Empty;

        /// <summary>
        /// Khởi tạo cấu hình cho Business Layer.
        /// Bắt buộc phải gọi hàm này 1 lần duy nhất từ Program.cs của tầng UI trước khi sử dụng bất kỳ Service nào.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server 2022.</param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy chuỗi tham số kết nối đến CSDL.
        /// Các Static Services trong hệ thống sẽ gọi thuộc tính này để cấp phát cho tầng Data.
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}