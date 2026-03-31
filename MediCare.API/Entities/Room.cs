using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public long DepartmentId { get; set; }
        public string RoomType { get; set; } = "General"; // General, Private, ICU, Emergency
        public string? Floor { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
    }
}
