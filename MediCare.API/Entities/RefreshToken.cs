using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class RefreshToken : BaseEntityWithCreatedOnly
    {
        public long UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
