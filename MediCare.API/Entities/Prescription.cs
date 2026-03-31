using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Prescription : BaseEntity
    {
        public long VisitId { get; set; }
        public long MedicationId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
        public long? UpdatedBy { get; set; }

        // Navigation
        public virtual Visit Visit { get; set; } = null!;
        public virtual Medication Medication { get; set; } = null!;
    }
}
