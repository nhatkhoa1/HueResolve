using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using HueResolve.Data.Interfaces;
using HueResolve.Data.SQLServer;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Lớp dịch vụ tĩnh quản lý các quy tắc nghiệp vụ cho tài khoản người dùng.
    /// Chịu trách nhiệm bảo mật thông tin và điều phối dữ liệu từ Repository.
    /// </summary>
    public static class UserService
    {
        private static IUserRepository _userRepository = default!;

        /// <summary>
        /// Khởi tạo tầng truy xuất dữ liệu người dùng.
        /// </summary>
        public static void Initialize(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);
        }

        /// <summary>
        /// Mã hóa chuỗi ký tự sang định dạng MD5 để bảo mật mật khẩu.
        /// </summary>
        public static string HashMD5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            StringBuilder sb = new();
            foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Xác thực thông tin đăng nhập của người dùng.
        /// Trả về User nếu username và mật khẩu khớp (kể cả tài khoản bị khóa).
        /// Caller có trách nhiệm kiểm tra IsActive để hiển thị thông báo phù hợp.
        /// </summary>
        public static async Task<User?> AuthenticateAsync(string username, string rawPassword)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null && user.PasswordHash == HashMD5(rawPassword))
            {
                return user;
            }
            return null;
        }

        /// <summary>
        /// Lấy danh sách người dùng phân trang phục vụ UI Admin.
        /// </summary>
        public static async Task<(IEnumerable<User> Data, int TotalCount)> GetPagedUsersAsync(int page, int pageSize, int? roleId, string? search)
        {
            return await _userRepository.GetPagedAsync(page, pageSize, roleId, search);
        }

        /// <summary>
        /// Lấy chi tiết thông tin một người dùng theo Id (Phục vụ validate quyền khóa tài khoản).
        /// </summary>
        public static async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Xử lý nghiệp vụ tạo tài khoản mới.
        /// </summary>
        public static async Task<bool> CreateUserAsync(User user, string rawPassword)
        {
            user.Id = Guid.NewGuid();
            user.CreatedAtUtc = DateTime.UtcNow;
            user.IsActive = true;
            user.PasswordHash = HashMD5(rawPassword);
            int result = await _userRepository.InsertAsync(user);
            return result > 0;
        }

        /// <summary>
        /// Cập nhật trạng thái khóa/mở khóa tài khoản.
        /// </summary>
        public static async Task<bool> SetUserActiveAsync(Guid userId, bool isActive)
        {
            int result = await _userRepository.UpdateIsActiveAsync(userId, isActive);
            return result > 0;
        }
        /// <summary>
        /// Lấy thông tin tài khoản thông qua tên đăng nhập (Username).
        /// Phục vụ cho việc kiểm tra trùng lặp khi người dân đăng ký tài khoản mới.
        /// </summary>
        public static async Task<User?> GetByUsernameAsync(string username)
        {
            /// Gọi trực tiếp hàm GetByUsernameAsync đã được định nghĩa trong IUserRepository
            /// Tuyệt đối không gọi nhầm sang GetByIdAsync() để tránh lỗi thiếu tham số Guid ID.
            return await _userRepository.GetByUsernameAsync(username);
        }

        /// <summary>
        /// Đổi mật khẩu cho tài khoản (có xác thực mật khẩu hiện tại).
        /// </summary>
        public static async Task<bool> ChangePasswordAsync(Guid userId, string currentRawPassword, string newRawPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Xác thực mật khẩu hiện tại
            if (user.PasswordHash != HashMD5(currentRawPassword))
                return false;

            // Cập nhật mật khẩu mới
            string newPasswordHash = HashMD5(newRawPassword);
            int result = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
            return result > 0;
        }

        /// <summary>
        /// Đặt lại mật khẩu cho tài khoản mà không cần xác thực mật khẩu cũ.
        /// Dành riêng cho Admin khi quản lý tài khoản người dùng khác.
        /// </summary>
        public static async Task<bool> ForceChangePasswordAsync(Guid userId, string newRawPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            string newPasswordHash = HashMD5(newRawPassword);
            int result = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
            return result > 0;
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân người dùng.
        /// </summary>
        public static async Task<bool> UpdateUserInfoAsync(Guid userId, string fullName, string? phoneNumber, string? addressText, string? avatarPath = null)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null) return false;

            var user = new User
            {
                Id = userId,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                AddressText = addressText,
                AvatarPath = avatarPath ?? existingUser.AvatarPath
            };
            int result = await _userRepository.UpdateAsync(user);
            return result > 0;
        }
    }
}