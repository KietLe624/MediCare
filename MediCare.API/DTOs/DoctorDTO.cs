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

        // RESPONSE
        public class DoctorSummaryResponse
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Specialty { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
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
            public List<ScheduleResponse> Schedules { get; set; } = new List<ScheduleResponse>();
        }

        public class DoctorScheduleResponse
        {
            [Required]
            public int Id { get; set; }
            public string DayOfWeek { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            [Required]
            public bool IsAvailable { get; set; }
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
    }
}
