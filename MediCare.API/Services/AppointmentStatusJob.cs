using MediCare.API.Data;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.AppointmentDTO;

namespace MediCare.API.Services
{
    public class AppointmentStatusJob : BackgroundService
    {
        private readonly ILogger<AppointmentStatusJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppointmentSettings _settings;
        public AppointmentStatusJob(ILogger<AppointmentStatusJob> logger, IServiceScopeFactory scopeFactory, AppointmentSettings settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settings = settings;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentStatusJob đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunAsync();
                }
                catch (Exception ex)
                {
                    // Không để lỗi crash cả job, chỉ log ra
                    _logger.LogError(ex, "Lỗi khi chạy AppointmentStatusJob.");
                }

                // Chờ 5 phút rồi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        private async Task RunAsync()
        {
            // Tạo scope mới để lấy DbContext (vì DbContext là scoped, không phải singleton)
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // --- Tự động NoShow ---
            // Nếu lịch đã qua giờ bắt đầu + NoShowAfterMinutes phút mà vẫn Confirmed → NoShow
            //var noShowTime = now.AddMinutes(-_settings.NoShowAfterMinutes);

            var noShowTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-_settings.NoShowAfterMinutes));
            var noShowList = await db.Appointments
                .Where(a =>
                    a.Status == "Confirmed" &&
                    a.StartTime <= noShowTime // Đã qua giờ hẹn
                )
                .ToListAsync();

            foreach (var a in noShowList)
            {
                a.Status = "NoShow";
                a.UpdatedAt = now;
                _logger.LogInformation("Auto NoShow: AppointmentId={Id}", a.Id);
            }

            await db.SaveChangesAsync();
        }
    }
}
