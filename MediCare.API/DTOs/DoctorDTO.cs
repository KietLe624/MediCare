using System.ComponentModel.DataAnnotations;
using static MediCare.API.DTOs.DoctorScheduleDTO;


namespace MediCare.API.DTOs
{
    public class DoctorDTO
    {
        public class DoctorQueryParams
        {
            public string? Search { get; set; }  // tìm theo tên, chuyên khoa, phòng ban, 
            public string ? Specialty { get; set; }
            public string? Department { get; set; }
            public string? Available { get; set; }  // "true" hoặc "false"
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortBy { get; set; } = "CreatedAt";
            public string SortOrder { get; set; } = "desc";
        }
        public class DoctorAppointmentQueryParams
        {
            public DateOnly? Date { get; set; }   // lọc theo ngày
            public string? Status { get; set; }   // Scheduled | Confirmed | Completed...
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
        }
        // RESPONSE
        public class DoctorLookupResponse
        {
            public long Id { get; set; }

            public string FullName { get; set; } = default!;

            public string? Specialization { get; set; }

            public bool IsAvailable { get; set; }
        }
        public class DoctorSummaryResponse
        {
            public int Id { get; set; }
            public string FullName { get; set; } = default!;
            public string Specialty { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string PhoneNumber { get; set; } = default!;
        }
        public class DoctorResponse
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Specialty { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Department { get; set; }
            public int ExperienceYears { get; set; }
            public decimal ConsultationFee { get; set; }
            public bool IsAvailable { get; set; }
            public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
            public List<DoctorScheduleResponse> Schedules { get; set; } = new List<DoctorScheduleResponse>();
        }
        public class DoctorScheduleResponse
        {
            [Required]
            public int Id { get; set; }
            public int DayOfWeek { get; set; } // "Monday", "Tuesday", ... 
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public int SlotDurationMinutes { get; set; } = 30; // mặc định 30 phút
            [Required]
            public bool IsActive { get; set; }
        }
        // REQUEST
        public class CreateDoctorRequest
        {
            [Required]
            public long UserId { get; set; }
            //[Required]
            public long? DepartmentId { get; set; }
            [StringLength(100)]
            public string? Specialization { get; set; }
            [StringLength(50)]
            public string? LicenseNumber { get; set; }
            [Range(0, 50)]
            public int? ExperienceYears { get; set; }
            
            public decimal ConsultationFee { get; set; }
            //[Required]
            public bool IsAvailable { get; set; } = true;
            [Required]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
        public class UpdateDoctorRequest
        {
            public long? DepartmentId { get; set; }
            [StringLength(100)]
            public string? Specialization { get; set; }
            [StringLength(50)]
            public string? LicenseNumber { get; set; }
            [Range(0, 50)]
            public int? ExperienceYears { get; set; }
            public decimal? ConsultationFee { get; set; }
            public bool? IsAvailable { get; set; }
            //public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        }
        public class PagedResponse<T>
        {
            public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
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
            //public int SlotDurationMinutes { get; set; } = 30;

            public bool IsActive { get; set; } = true;
        }
        public class ScheduleStatusRequest
        {
            public bool IsAvailable { get; set; } // "true" hoặc "false"
        }
        public class AvailableSlotsResponse
        {
            public long DoctorId { get; set; }
            public string DoctorName { get; set; } = default!;
            public DateOnly Date { get; set; }
            public List<TimeSlotResponse> Slots { get; set; } = new();
        }
        public class TimeSlotResponse
        {
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public bool IsAvailable { get; set; } // false nếu đã có appointment
        }
        public class DoctorAppointmentResponse
        {
            public long Id { get; set; }
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string Status { get; set; } = default!;
            public string? Reason { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public PatientBriefResponse Patient { get; set; } = default!;
        }
        public class PatientBriefResponse
        {
            public long Id { get; set; }
            public string UHID { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public string? PhoneNumber { get; set; }
        }
        public class DepartmentBriefResponse
        {
            public long Id { get; set; }
            public string Name { get; set; } = default!;
        }
    }
}
