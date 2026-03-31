using System.ComponentModel.DataAnnotations;
using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        [Required]
        public long DoctorId { get; set; }
        //1 = Monday, 2 = Tuesday, ..., 7 = Sunday
        [Required]
        [Range(1, 7)]
        public int DayOfWeek { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual Doctor Doctor { get; set; } = null!;

    }
}