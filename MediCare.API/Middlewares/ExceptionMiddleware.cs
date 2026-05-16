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
            int statusCode;
            string message;

            switch (exception)
            {
                case AppException appEx:
                    statusCode = appEx.StatusCode;
                    message = appEx.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case BadHttpRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = "Bạn không có quyền truy cập";
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "Đã xảy ra lỗi trên server";
                    break;
            }

            var response = new
            {
                statusCode,
                message
            };

            var json = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(json);
        }

        public class AppException : Exception
        {
            public int StatusCode { get; }

            public AppException(string message, int statusCode = 400)
                : base(message)
            {
                StatusCode = statusCode;
            }
        }
    }
}
