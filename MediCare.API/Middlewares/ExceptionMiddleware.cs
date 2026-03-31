using System.Net;
using System.Text.Json;

namespace MediCare.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp vào Controller và Service
                await _next(context);
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào bị throw ra, nó sẽ rơi vào lưới này
                _logger.LogError(ex, "Đã xảy ra lỗi: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Tạo một response chuẩn với mã lỗi và thông điệp lỗi
            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Đã xảy ra lỗi trên server. Vui lòng thử lại sau."
            };
            // Chuyển đổi response thành JSON
            var jsonResponse = JsonSerializer.Serialize(response);
            // Thiết lập header và status code cho response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            // Gửi response về client
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
