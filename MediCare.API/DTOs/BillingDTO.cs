using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class BillingDTO
    {
        // ─── QUERY PARAMS ─────────────────────────────────────────

        public class InvoiceQueryParams
        {
            public long? PatientId { get; set; }
            public string? Status { get; set; }  // Draft | Issued | Paid | PartialPaid | Cancelled
            public string? PaymentMethod { get; set; }  // Cash | Insurance | Mixed
            public DateOnly? FromDate { get; set; }
            public DateOnly? ToDate { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string SortOrder { get; set; } = "desc";
        }

        // ─── REQUEST DTOs ─────────────────────────────────────────

        public class CreateInvoiceRequest
        {
            [Required(ErrorMessage = "Bệnh nhân là bắt buộc")]
            public long PatientId { get; set; }

            // VisitId nullable — hóa đơn có thể tạo độc lập không gắn với visit
            public long? VisitId { get; set; }

            [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
            public string PaymentMethod { get; set; } = default!;

            [Range(0, double.MaxValue, ErrorMessage = "Giảm giá không hợp lệ")]
            public decimal DiscountAmount { get; set; } = 0;

            [Range(0, double.MaxValue, ErrorMessage = "Thuế không hợp lệ")]
            public decimal TaxAmount { get; set; } = 0;

            [StringLength(500)]
            public string? Notes { get; set; }

            // Các dòng hóa đơn — phải có ít nhất 1 item
            [Required]
            [MinLength(1, ErrorMessage = "Hóa đơn phải có ít nhất 1 dòng")]
            public List<CreateInvoiceItemRequest> Items { get; set; } = new();
        }

        public class CreateInvoiceItemRequest
        {
            [Required(ErrorMessage = "Loại dịch vụ là bắt buộc")]
            [RegularExpression("^(Consultation|Medication|LabTest|Other)$",
                ErrorMessage = "ItemType không hợp lệ")]
            public string ItemType { get; set; } = default!;

            [Required(ErrorMessage = "Mô tả là bắt buộc")]
            [StringLength(255)]
            public string Description { get; set; } = default!;

            [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
            public decimal Quantity { get; set; } = 1;

            [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không hợp lệ")]
            public decimal UnitPrice { get; set; }

            // RefId: MedicationId nếu ItemType = Medication, null cho loại khác
            public long? RefId { get; set; }
        }

        public class AddInvoiceItemRequest
        {
            [Required]
            [RegularExpression("^(Consultation|Medication|LabTest|Other)$")]
            public string ItemType { get; set; } = default!;

            [Required]
            [StringLength(255)]
            public string Description { get; set; } = default!;

            [Range(0.01, double.MaxValue)]
            public decimal Quantity { get; set; } = 1;

            [Range(0, double.MaxValue)]
            public decimal UnitPrice { get; set; }

            public long? RefId { get; set; }
        }

        public class UpdateInvoiceItemRequest
        {
            [Required]
            [StringLength(255)]
            public string Description { get; set; } = default!;

            [Range(0.01, double.MaxValue)]
            public decimal Quantity { get; set; } = 1;

            [Range(0, double.MaxValue)]
            public decimal UnitPrice { get; set; }
        }

        public class PayInvoiceRequest
        {
            [Required(ErrorMessage = "Số tiền thanh toán là bắt buộc")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
            public decimal PaidAmount { get; set; }

            [RegularExpression("^(Cash|Insurance|Mixed)$")]
            public string? PaymentMethod { get; set; } // override nếu cần đổi
        }

        // ─── RESPONSE DTOs ────────────────────────────────────────

        public class InvoiceResponse
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

            public PatientBriefResponse Patient { get; set; } = default!;
            public List<InvoiceItemResponse> Items { get; set; } = new();
        }

        public class InvoiceSummaryResponse
        {
            public long Id { get; set; }
            public string InvoiceNumber { get; set; } = default!;
            public string Status { get; set; } = default!;
            public string PaymentMethod { get; set; } = default!;
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal RemainingAmount { get; set; }
            public DateTime? IssuedAt { get; set; }
            public DateTime CreatedAt { get; set; }

            public PatientBriefResponse Patient { get; set; } = default!;
        }

        public class InvoiceItemResponse
        {
            public long Id { get; set; }
            public string ItemType { get; set; } = default!;
            public string Description { get; set; } = default!;
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public long? RefId { get; set; }
        }

        // ─── REVENUE REPORT ───────────────────────────────────────

        public class RevenueReportResponse
        {
            public decimal TotalRevenue { get; set; } // tổng doanh thu (Paid + PartialPaid)
            public decimal TotalPaid { get; set; } // đã thu được
            public decimal TotalRemaining { get; set; } // còn nợ
            public int TotalInvoices { get; set; } // tổng số hóa đơn
            public int PaidCount { get; set; }
            public int PartialPaidCount { get; set; }
            public int PendingCount { get; set; } // Draft + Issued

            // Breakdown theo phương thức thanh toán
            public decimal CashRevenue { get; set; }
            public decimal InsuranceRevenue { get; set; }
            public decimal MixedRevenue { get; set; }
        }

        // ─── BRIEF RESPONSES ──────────────────────────────────────

        public class PatientBriefResponse
        {
            public long Id { get; set; }
            public string UHID { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public long? UserId { get; set; }
        }
    }
}
