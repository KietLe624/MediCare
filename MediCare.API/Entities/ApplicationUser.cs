using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        [Required]
        public override string UserName { get; set; } = null!; // use Identity's UserName

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public override string? PhoneNumber { get; set; } 

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Patient? Patient { get; set; }
        public virtual Doctor? Doctor { get; set; }
    }
}
