using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Visit : BaseEntity
    {
        public long AppointmentId { get; set; }
        public DateTime VisitDate { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; } 
        public string? Treatment { get; set; }
        public string? Notes { get; set; }

        public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled

        // Navigation
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
