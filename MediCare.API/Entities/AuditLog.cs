using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class AuditLog : BaseEntityWithCreatedOnly
    {
        public long Id { get; set; }
        public long? UserId { get; set; }     // nullable: system action không có user
        public string? UserName { get; set; }     // snapshot tên lúc log — tránh mất khi user bị xóa
        public string Action { get; set; } = default!;     // Create | Update | Delete | Login | Logout
        public string EntityName { get; set; } = default!;     // "Patient", "Appointment"...
        public string? EntityId { get; set; }    
        public string? OldValues { get; set; }    
        public string? NewValues { get; set; }    
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
