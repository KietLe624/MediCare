using MediCare.API.Entities;

namespace MediCare.API.Common
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; } // Người tạo
        public long? UpdatedBy { get; set; } // Người cập nhật
    }
    public abstract class BaseEntityWithCreatedOnly
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
    }
}
