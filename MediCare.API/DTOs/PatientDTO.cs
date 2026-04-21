using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class PatientDTO
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


    }

}