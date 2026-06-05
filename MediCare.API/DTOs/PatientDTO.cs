using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class PatientQueryParams
    {
        public string? Search { get; set; }  // tìm theo tên, UHID, SĐT
        public string? PatientType { get; set; }  // Outpatient | Inpatient
        public string? BloodType { get; set; }
        public string? Gender { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }
    public class PatientHistoryQueryParams
    {
        public string? Search { get; set; }  // tìm theo tên bác sĩ, phòng ban, lý do khám
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "Date"; // AppointmentDate, VisitDate, PrescriptionDate, InvoiceDate
        public string SortOrder { get; set; } = "desc";
        //public List<PatientVisitResponse> Visits { get; internal set; }
        public int TotalCount { get; internal set; }
    }
    // REQUEST
    public class CreatePatientRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = default!;

        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public string Gender { get; set; } = default!;
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [RegularExpression("^(A|B|AB|O)[+-]$",
            ErrorMessage = "Nhóm máu không hợp lệ. Ví dụ: A+, B-, AB+, O-")]
        public string? BloodType { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "SĐT liên hệ khẩn cấp không hợp lệ")]
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [RegularExpression("^(Outpatient|Inpatient)$",
            ErrorMessage = "Loại bệnh nhân phải là Outpatient hoặc Inpatient")]
        public string PatientType { get; set; } = "Outpatient";

        [StringLength(100)]
        public string? InsuranceProvider { get; set; }

        [StringLength(50)]
        public string? InsuranceNumber { get; set; }

        // UserId nullable — bệnh nhân có thể không có tài khoản
        public long? UserId { get; set; }
    }
    public class UpdatePatientRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = default!;

        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public string Gender { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [RegularExpression("^(A|B|AB|O)[+-]$",
            ErrorMessage = "Nhóm máu không hợp lệ. Ví dụ: A+, B-, AB+, O-")]
        public string? BloodType { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [RegularExpression("^(Outpatient|Inpatient)$",
            ErrorMessage = "Loại bệnh nhân phải là Outpatient hoặc Inpatient")]
        public string PatientType { get; set; } = "Outpatient";

        [StringLength(100)]
        public string? InsuranceProvider { get; set; }

        [StringLength(50)]
        public string? InsuranceNumber { get; set; }

        // UserId nullable — bệnh nhân có thể không có tài khoản
        public long? UserId { get; set; }
    }
    // RESPONSE
    public class PatientLookupResponse
    {
        public long Id { get; set; }
        public string UHID { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? PhoneNumber { get; set; }
    }
    public class PatientResponse
    {
        public long Id { get; set; }
        public string UHID { get; set; } = default!;
        public long? UserId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string FullName { get; set; } = default!; // FirstName + LastName
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }             // tính từ DateOfBirth
        public string Gender { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string PatientType { get; set; } = default!;
        public string? InsuranceProvider { get; set; }
        public string? InsuranceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class PatientSummaryResponse
    {
        public long Id { get; set; }
        public string UHID { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? BloodType { get; set; }
        public string PatientType { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
    public class PatientAppointmentResponse
    {
        public long Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; } = default!;
        public string Status { get; set; } = default!; // Scheduled, Completed, Cancelled
        public string StartTime { get; internal set; }
        public string EndTime { get; internal set; }
        public string? Reason { get; internal set; }
        public string? Notes { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DoctorBriefResponse Doctor { get; internal set; }
        public DepartmentBriefResponse Department { get; internal set; }
    }
    public class PatientVisitResponse
    {
        public long Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string Department { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public string? Symptoms { get; internal set; }
        public string? Diagnosis { get; internal set; }
        public string? Treatment { get; internal set; }
        public string? Notes { get; internal set; }
        public string Status { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DoctorBriefResponse Doctor { get; internal set; }
        public int PrescriptionCount { get; internal set; }
    }
    public class PatientPrescriptionResponse
    {
        public long Id { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string DoctorName { get; set; } = default!;
        public string Medication { get; set; } = default!;
        public string Dosage { get; set; } = default!;
        public string Instructions { get; set; } = default!;
        public int Unit { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public string Frequency { get; internal set; }
        public string? Duration { get; internal set; }
        public long VisitId { get; internal set; }
        public DateTime VisitDate { get; internal set; }
        public DoctorBriefResponse Doctor { get; internal set; }
    }
    public class PatientInvoiceResponse
    {
        public long Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string PaymentMethod { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; } // TotalAmount - PaidAmount
        public string? Notes { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        //public List<InvoiceItemResponse> Items { get; set; } = new();
    }
    public class DoctorBriefResponse
    {
        public long Id { get; set; }
        public string? FullName { get; set; } = default!;
        public string? Specialization { get; set; }
        public decimal? ConsultationFee { get; set; }
        public bool IsAvailable { get; set; }
    }
    public class DepartmentBriefResponse
    {
        public long Id { get; set; }
        public string? Name { get; set; } = default!;
    }
    public class MedicationBriefResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string GenericName { get; set; }
        public string Unit { get; set; }
    }
    public class InvoiceItemBriefResponse
    {
        public long Id { get; set; }
        public string ItemType { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
