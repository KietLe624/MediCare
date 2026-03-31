using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Bed : BaseEntity
    {
        public string BedNumber { get; set; } = string.Empty;
        public long RoomId { get; set; }
        public string Status { get; set; } = "Available"; // Available, Occupied, Maintenance

        // Navigation
        public virtual Room Room { get; set; } = null!;
        public virtual ICollection<BedAssignment> BedAssignments { get; set; } = new List<BedAssignment>();
    }
}
