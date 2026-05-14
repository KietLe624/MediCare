using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class DepartmentDTO
    
    {
        // QUERY PARAMS

        public class DepartmentQueryParams
        {
            public string? Search { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortBy { get; set; } = "Name";
            public string SortOrder { get; set; } = "asc";
        }

        // REQUEST 

        public class CreateDepartmentRequest
        {
            [Required(ErrorMessage = "Tên khoa là bắt buộc")]
            [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
            public string Name { get; set; } = default!;

            [StringLength(500)]
            public string? Description { get; set; }
        }

        public class UpdateDepartmentRequest
        {
            [Required(ErrorMessage = "Tên khoa là bắt buộc")]
            [StringLength(100)]
            public string Name { get; set; } = default!;

            [StringLength(500)]
            public string? Description { get; set; }
        }

        // RESPONSE

        public class DepartmentResponse
        {
            public long Id { get; set; }
            public string Name { get; set; } = default!;
            public string? Description { get; set; }
            public int DoctorCount { get; set; } // số bác sĩ đang thuộc khoa
            public DateTime CreatedAt { get; set; }
        }

        public class DepartmentDetailResponse : DepartmentResponse
        {
            // Danh sách bác sĩ trong khoa — dùng cho GET /departments/{id}
            public List<DoctorBriefResponse> Doctors { get; set; } = new();
        }

        public class DoctorBriefResponse
        {
            public long Id { get; set; }
            public string FullName { get; set; } = default!;
            public string? Specialization { get; set; }
            public decimal? ConsultationFee { get; set; }
            public bool IsAvailable { get; set; }
        }
    }
}
