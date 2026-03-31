using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class BedAssignment : BaseEntity
    {
        public long BedId { get; set; }
        public long PatientId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DischargedAt { get; set; }
        public string? Notes { get; set; }
        public long? AssignedBy { get; set; }

        // Navigation
        public virtual Bed Bed { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
