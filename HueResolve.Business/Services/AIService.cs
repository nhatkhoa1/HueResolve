using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HueResolve.Models.Model;
using Newtonsoft.Json;

namespace HueResolve.Business.Services
{
    /// <summary>
    /// Service giao tiếp với AI Microservice (FastAPI) qua HTTP RESTful API.
    /// Sử dụng Singleton HttpClient với Timeout tường minh để tránh Socket Exhaustion.
    /// </summary>
    public static class AIService
    {
        private const string AI_MICROSERVICE_URL = "http://127.0.0.1:8000";

        // Dùng Lazy để khởi tạo một lần duy nhất, thread-safe
        private static readonly Lazy<HttpClient> _lazyClient = new(() =>
        {
            var client = new HttpClient
            {
                // Timeout ngắn để không block request của người dùng khi AI chưa chạy
                Timeout = TimeSpan.FromSeconds(5)
            };
            return client;
        });

        private static HttpClient HttpClient => _lazyClient.Value;

        /// <summary>
        /// Kiểm tra AI Microservice có đang hoạt động không.
        /// </summary>
        public static async Task<bool> IsAvailableAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync($"{AI_MICROSERVICE_URL}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi dữ liệu sang AI Microservice để phân loại danh mục sự cố.
        /// Trả về null nếu AI không khả dụng hoặc độ tin cậy quá thấp.
        /// </summary>
        public static async Task<Category?> PredictCategoryAsync(string title, string description)
        {
            try
            {
                var categories = await CategoryService.GetAllCategoriesAsync();
                if (!categories.Any()) return null;

                var payload = new
                {
                    title,
                    description,
                    categories = categories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        code = c.Code ?? "",
                        description = c.Description ?? ""
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync($"{AI_MICROSERVICE_URL}/predict_category", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<AiCategoryResponse>(responseData);

                    if (result != null)
                    {
                        // Chỉ tin vào kết quả AI nếu confidence đủ cao (>= 50%)
                        if (result.confidence >= 0.50)
                        {
                            return categories.FirstOrDefault(c => c.Id == result.category_id);
                        }

                        Console.WriteLine($"[AI] Độ tin cậy thấp ({result.confidence:P0}), trạng thái: {result.state}. Bỏ qua kết quả AI.");
                        return null;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[AI] Timeout khi gọi AI service. Có thể chưa khởi động.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[AI] Không kết nối được AI service: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI] Lỗi không xác định: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gợi ý danh sách Đơn vị xử lý phù hợp, kèm confidence và lý do.
        /// Trả về null nếu AI không khả dụng.
        /// </summary>
        public static async Task<List<AiSuggestion>?> SuggestDepartmentAsync(string title, string description, int? categoryId)
        {
            try
            {
                var departments = await DepartmentService.GetActiveDepartmentsAsync();
                if (!departments.Any()) return null;

                var payload = new
                {
                    title,
                    description,
                    category_id = categoryId,
                    departments = departments.Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        type = d.Type ?? ""
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync($"{AI_MICROSERVICE_URL}/suggest_department", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<AiSuggestListResponse>(responseData);
                    return result?.suggestions;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[AI] Timeout khi gọi suggest_department.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[AI] Không kết nối được AI service: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI] Lỗi không xác định: {ex.Message}");
            }

            return null;
        }

        // ── Response models ─────────────────────────────────────────────────────

        private class AiCategoryResponse
        {
            public int category_id { get; set; }
            public double confidence { get; set; }
            public string state { get; set; } = "";
        }

        private class AiSuggestListResponse
        {
            public List<AiSuggestion> suggestions { get; set; } = new();
        }

        public class AiSuggestion
        {
            public int id { get; set; }
            public string name { get; set; } = "";
            public double confidence { get; set; }
            public string reason { get; set; } = "";
        }
    }
}
