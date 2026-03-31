using MediCare.API.Common;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.Entities
{
    public class Appointment : BaseEntity
    {
        [Required]
        public long PatientId { get; set; }
        [Required]
        public long DoctorId { get; set; }
        [Required]
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled, NoShow, Rescheduled
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        // Navigation
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}
