using MediCare.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace MediCare.API.Interceptor
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Các entity không cần log 
        private static readonly HashSet<string> ExcludedEntities = new()
        {
            nameof(AuditLog),
            nameof(RefreshToken),
        };

        private static readonly HashSet<string> ExcludedProperties = new()
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "AccessFailedCount",
            "UpdatedAt",
            "UpdatedBy"
        };

        public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Ghi đè phương thức SavingChangesAsync để chèn logic tạo audit log trước khi EF lưu thay đổi vào database
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // kiểm tra dbcontext 
            if (eventData.Context is null)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var auditLogs = BuildAuditLogs(eventData.Context);

            // Thêm AuditLog vào context — sẽ được lưu cùng lúc với entity
            if (auditLogs.Any())
                await eventData.Context.Set<AuditLog>().AddRangeAsync(auditLogs, cancellationToken);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private List<AuditLog> BuildAuditLogs(DbContext context)
        {
            var logs = new List<AuditLog>();
            var userId = GetCurrentUserId(); // lấy userId
            var userName = GetCurrentUserName(); // lấy userName
            var ipAddress = GetIpAddress(); // lấy ipAddress
            var userAgent = GetUserAgent(); // lấy userAgent

            // Duyệt qua tất cả entity đang được track có thay đổi
            foreach (var entry in context.ChangeTracker.Entries())
            {
                // Bỏ qua entity không thay đổi
                if (entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Detached)
                    continue;

                var entityName = entry.Entity.GetType().Name;

                // Bỏ qua entity trong danh sách không cần log
                if (ExcludedEntities.Contains(entityName))
                    continue;

                var action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => null
                };
                // Bỏ qua nếu không xác định được action 
                if (action is null) continue;

                // Lấy Id của entity
                var entityId = entry.Properties
                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                    ?.CurrentValue
                    ?.ToString();

                // OldValues: snapshot trước khi thay đổi (chỉ Update / Delete)
                string? oldValues = null;
                if (entry.State == EntityState.Modified ||
                    entry.State == EntityState.Deleted)
                {
                    var oldDict = entry.Properties
                        .Where(p => (p.IsModified || entry.State == EntityState.Deleted) &&
                                     !ExcludedProperties.Contains(p.Metadata.Name))
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => p.OriginalValue);

                    oldValues = JsonSerializer.Serialize(oldDict);
                }

                // NewValues: snapshot sau khi thay đổi (chỉ Create / Update)
                string? newValues = null;
                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified)
                {
                    var newDict = entry.Properties
                        .Where(p => (p.IsModified || entry.State == EntityState.Added) &&
                                     !ExcludedProperties.Contains(p.Metadata.Name))
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue);

                    newValues = JsonSerializer.Serialize(newDict);
                }

                logs.Add(new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return logs;
        }

        private long? GetCurrentUserId()
        {
            var sub = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(sub, out var id) ? id : null;
        }

        private string? GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Name);
        }

        private string? GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null) return null;

            // Lấy IP thật nếu đứng sau reverse proxy (nginx, load balancer)
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?
                .Request.Headers["User-Agent"].FirstOrDefault();
        }
    }
}
