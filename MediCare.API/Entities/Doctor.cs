using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Doctor : BaseEntity
    {
        public long UserId { get; set; }
        public long DepartmentId { get; set; }

        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
