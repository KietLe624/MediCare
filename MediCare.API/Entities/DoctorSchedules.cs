using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediCare.API.Common;
// Đã xóa using MimeKit...

namespace MediCare.API.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long DoctorId { get; set; }

        [Required]
        [Range(1, 7)]
        public int DayOfWeek { get; set; } // 1 = Monday, ..., 7 = Sunday

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30; // mặc định 30 phút
        public bool IsActive { get; set; } = true;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}