namespace MediCare.API.DTOs
{
    public class DoctorScheduleDTO
    {
        public class ScheduleResponse
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public bool IsAvailable { get; set; } // trạng thái lịch trình có sẵn hay không
        }
        public class CreateScheduleRequest
        {
            public int DoctorId { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; } 
        }
        public class UpdateScheduleRequest
        {
            public DateTime? Date { get; set; }
            public TimeSpan? StartTime { get; set; }
            public TimeSpan? EndTime { get; set; }
            public bool? IsAvailable { get; set; } // "true" hoặc "false" 
        }
        public class  ScheduleStatusRequest
        {
            public bool IsAvailable { get; set; } // "true" hoặc "false"
        }
    }
}
