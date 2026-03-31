using MediCare.API.Common;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.Entities
{
    public class Patient : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string UHID { get; set; } = string.Empty;
        public long? UserId { get; set; }
        [Required] 
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string PatientType { get; set; } = "Outpatient";
        public string? InsuranceProvider { get; set; }
        public string? InsuranceNumber { get; set; }
        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<BedAssignment> BedAssignments { get; set; } = new List<BedAssignment>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}