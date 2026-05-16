using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class MedicationDTO

    {
        // QUERY PARAMS 

        public class MedicationQueryParams
        {
            public string? Search { get; set; }   // tìm theo tên, generic name
            public string? Category { get; set; }   // lọc theo danh mục
            public bool? IsActive { get; set; }   // true = đang dùng, false = ngừng
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortBy { get; set; } = "Name";
            public string SortOrder { get; set; } = "asc";
        }

        // REQUEST

        public class CreateMedicationRequest
        {
            [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
            [StringLength(200, ErrorMessage = "Tên thuốc tối đa 200 ký tự")]
            public string Name { get; set; } = default!;

            [StringLength(200)]
            public string? GenericName { get; set; }  // tên hoạt chất

            [StringLength(100)]
            public string? Category { get; set; }     // Antibiotic, Analgesic, Antiviral...

            [StringLength(50)]
            public string? Unit { get; set; }         // đơn vị tính: mg, ml, tablet, viên...

            [StringLength(500)]
            public string? Description { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public class UpdateMedicationRequest
        {
            [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
            public string Name { get; set; } = default!;

            [StringLength(200)]
            public string? GenericName { get; set; }

            [StringLength(100)]
            public string? Category { get; set; }

            [StringLength(50)]
            public string? Unit { get; set; }

            [StringLength(500)]
            public string? Description { get; set; }

            public bool IsActive { get; set; } = true;
        }

        // RESPONSE 

        public class MedicationResponse
        {
            public long Id { get; set; }
            public string Name { get; set; } = default!;
            public string? GenericName { get; set; }
            public string? Category { get; set; }
            public string? Unit { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Danh sách các danh mục thuốc hiện có trong hệ thống
        public class MedicationCategoryResponse
        {
            public string Name { get; set; } = default!;
            public int Count { get; set; } // số thuốc trong danh mục
        }
    }
}

