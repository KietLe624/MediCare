using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }   // ghi đè nếu cần

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Patient? Patient { get; set; }     // bệnh nhân
        public virtual Doctor? Doctor { get; set; }       // bác sĩ
    }
}
