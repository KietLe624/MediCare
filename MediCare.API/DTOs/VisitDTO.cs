using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class VisitDTO
    {
        public class VisitQueryParams
        {
            public long? PatientId { get; set; }
            public long? DoctorId { get; set; }
            public DateOnly? FromDate { get; set; }
            public DateOnly? ToDate { get; set; }
            public string? Status { get; set; }  // InProgress | Completed | Cancelled
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortOrder { get; set; } = "desc";
        }

        // REQUEST

        public class CreateVisitRequest
        {
            [Required(ErrorMessage = "AppointmentId là bắt buộc")]
            public long AppointmentId { get; set; }

            // VisitDate mặc định là lúc bắt đầu khám — có thể override nếu cần
            public DateTime? VisitDate { get; set; }

            [StringLength(1000)]
            public string? Symptoms { get; set; }

            [StringLength(1000)]
            public string? Diagnosis { get; set; }

            [StringLength(1000)]
            public string? Treatment { get; set; }

            public string? Notes { get; set; }
        }

        public class UpdateVisitRequest
        {
            [StringLength(1000)]
            public string? Symptoms { get; set; }

            [StringLength(1000)]
            public string? Diagnosis { get; set; }

            [StringLength(1000)]
            public string? Treatment { get; set; }

            public string? Notes { get; set; }
        }

        // PRESCRIPTION

        public class AddPrescriptionRequest
        {
            [Required(ErrorMessage = "Thuốc là bắt buộc")]
            public long MedicationId { get; set; }

            [Required(ErrorMessage = "Liều dùng là bắt buộc")]
            [StringLength(100)]
            public string Dosage { get; set; } = default!;

            [Required(ErrorMessage = "Tần suất là bắt buộc")]
            [StringLength(100)]
            public string Frequency { get; set; } = default!;

            [StringLength(100)]
            public string? Duration { get; set; }

            [StringLength(500)]
            public string? Instructions { get; set; }
        }

        public class UpdatePrescriptionRequest
        {
            [Required]
            public long MedicationId { get; set; }

            [Required]
            [StringLength(100)]
            public string Dosage { get; set; } = default!;

            [Required]
            [StringLength(100)]
            public string Frequency { get; set; } = default!;

            [StringLength(100)]
            public string? Duration { get; set; }

            [StringLength(500)]
            public string? Instructions { get; set; }
        }

        // RESPONSE DTOs 

        public class VisitResponse
        {
            public long Id { get; set; }
            public long AppointmentId { get; set; }
            public DateTime VisitDate { get; set; }
            public string? Symptoms { get; set; }
            public string? Diagnosis { get; set; }
            public string? Treatment { get; set; }
            public string? Notes { get; set; }
            public string Status { get; set; } = default!;
            public DateTime CreatedAt { get; set; }

            public PatientBriefResponse Patient { get; set; } = default!;
            public DoctorBriefResponse Doctor { get; set; } = default!;

            // Đơn thuốc trong lần khám này
            public List<PrescriptionResponse> Prescriptions { get; set; } = new();
        }

        public class VisitSummaryResponse
        {
            public long Id { get; set; }
            public long AppointmentId { get; set; }
            public DateTime VisitDate { get; set; }
            public string? Diagnosis { get; set; }
            public string Status { get; set; } = default!;
            public int PrescriptionCount { get; set; }
            public DateTime CreatedAt { get; set; }

            public PatientBriefResponse Patient { get; set; } = default!;
            public DoctorBriefResponse Doctor { get; set; } = default!;
        }

        // PRESCRIPTION RESPONSE 

        public class PrescriptionResponse
        {
            public long Id { get; set; }
            public long VisitId { get; set; }
            public string Dosage { get; set; } = default!;
            public string Frequency { get; set; } = default!;
            public string? Duration { get; set; }
            public string? Instructions { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public MedicationBriefResponse Medication { get; set; } = default!;
        }

        // BRIEF RESPONSES

        public class PatientBriefResponse
        {
            public long Id { get; set; }
            public string UHID { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public long? UserId { get; set; }
        }

        public class DoctorBriefResponse
        {
            public long Id { get; set; }
            public string FullName { get; set; } = default!;
            public string? Specialization { get; set; }
            public string Department { get; set; } = default!;
        }

        public class MedicationBriefResponse
        {
            public long Id { get; set; }
            public string Name { get; set; } = default!;
            public string? GenericName { get; set; }
            public string? Unit { get; set; }
            public string? Category { get; set; }
        }
    }
}
