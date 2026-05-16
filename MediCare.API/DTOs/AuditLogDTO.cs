namespace MediCare.API.DTOs
{
    // QUERY 

    public class AuditLogQueryParams
    {
        public long? UserId { get; set; }
        public string? Action { get; set; }   // Create | Update | Delete | Login | Logout
        public string? EntityName { get; set; }   // Patient | Appointment | Visit...
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // RESPONSE

    public class AuditLogResponse
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = default!;
        public string EntityName { get; set; } = default!;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }  // JSON string — Angular tự parse
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}