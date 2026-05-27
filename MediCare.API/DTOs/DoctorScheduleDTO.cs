using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class DoctorScheduleDTO
    {
        public class ScheduleResponse
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public DateTime Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public int SlotDurationMinutes { get; set; } = 30; // mặc định 30 phút
            public bool IsAvailable { get; set; } // trạng thái lịch trình có sẵn hay không
        }
        public class CreateScheduleRequest
        {
            [Range(1, 7, ErrorMessage = "DayOfWeek: 1=Thứ 2 ... 7=Chủ nhật")]
            public int DayOfWeek { get; set; }

            [Required]
            public TimeOnly StartTime { get; set; }

            [Required]
            public TimeOnly EndTime { get; set; }

            //[Range(15, 120)]
            public int SlotDurationMinutes { get; set; } = 30; // mặc định 30 phút

            public bool IsActive { get; set; } = true;
        }

        public class UpdateScheduleRequest
        {
            [Required]
            [Range(1, 7)]
            public int DayOfWeek { get; set; }

            [Required]
            public TimeOnly StartTime { get; set; }

            [Required]
            public TimeOnly EndTime { get; set; }

            //[Range(15, 120)]
            public int SlotDurationMinutes { get; set; } = 30;

            public bool IsActive { get; set; } = true;
        }
        public class  ScheduleStatusRequest
        {
            public bool IsAvailable { get; set; } // "true" hoặc "false"
        }
    }
}
