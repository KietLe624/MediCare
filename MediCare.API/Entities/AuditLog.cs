using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class AuditLog : BaseEntityWithCreatedOnly
    {
        public long? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
    }
}
