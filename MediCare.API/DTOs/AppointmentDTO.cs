using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class AppointmentDTO
    {
        // QUERY 
        public class AppointmentQueryParams
        {
            public DateOnly? Date { get; set; }   // lọc theo ngày
            public long? DoctorId { get; set; }
            public long? PatientId { get; set; }
            public string? Status { get; set; }   // Scheduled | Confirmed | Completed...
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortOrder { get; set; } = "desc";
        }

        public class CreateAppointmentRequest
        {
            [Required(ErrorMessage = "Bệnh nhân là bắt buộc")]
            public long PatientId { get; set; }

            [Required(ErrorMessage = "Bác sĩ là bắt buộc")]
            public long DoctorId { get; set; }

            [Required(ErrorMessage = "Ngày khám là bắt buộc")]
            public DateOnly AppointmentDate { get; set; }

            [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
            public TimeOnly StartTime { get; set; }

            [StringLength(255)]
            public string? Reason { get; set; }

            [StringLength(500)]
            public string? Notes { get; set; }
        }

        public class UpdateAppointmentRequest
        {
            [Required(ErrorMessage = "Ngày khám là bắt buộc")]
            public DateOnly AppointmentDate { get; set; }

            [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
            public TimeOnly StartTime { get; set; }

            [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
            public TimeOnly EndTime { get; set; }

            [StringLength(255)]
            public string? Reason { get; set; }

            [StringLength(500)]
            public string? Notes { get; set; }
        }

        public class RescheduleAppointmentRequest
        {
            [Required(ErrorMessage = "Ngày khám mới là bắt buộc")]
            public DateOnly NewDate { get; set; }

            [Required(ErrorMessage = "Giờ bắt đầu mới là bắt buộc")]
            public TimeOnly NewStartTime { get; set; }

            //[Required(ErrorMessage = "Giờ kết thúc mới là bắt buộc")]
            //public TimeOnly NewEndTime { get; set; }

            [StringLength(500)]
            public string? Reason { get; set; } // lý do đổi lịch
        }

        public class CancelAppointmentRequest
        {
            [StringLength(500)]
            public string? Reason { get; set; } // lý do hủy
        }

        // RESPONSE
        public class AppointmentResponse
        {
            public long Id { get; set; }
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string? Notes { get; set; } = null;
            public string? Reason { get; set; }
            public string Status { get; set; } = default!;
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public PatientBriefResponse Patient { get; set; } = default!;
            public DoctorBriefResponse Doctor { get; set; } = default!;
            public DepartmentBriefResponse Department { get; set; } = default!;

        }
        public class AppointmentSummaryResponse
        {
            public long Id { get; set; }
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string Status { get; set; } = default!;
            public string? Reason { get; set; }

            public PatientBriefResponse Patient { get; set; } = default!;
            public DoctorBriefResponse Doctor { get; set; } = default!;
        }

        // ─── BRIEF RESPONSES ──────────────────────────────────────

        public class PatientBriefResponse
        {
            public long Id { get; set; }
            public long? UserId { get; set; }
            public string UHID { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public string? PhoneNumber { get; set; }

        }

        public class DoctorBriefResponse
        {
            public long Id { get; set; }
            public string FullName { get; set; } = default!;
            public string? Specialization { get; set; }
        }

        public class DepartmentBriefResponse
        {
            public long Id { get; set; }
            public string Name { get; set; } = default!;
        }
    }
}
