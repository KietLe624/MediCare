using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public long? VisitId { get; set; }

        public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, PartialPaid, Cancelled, Refunded
        public string? PaymentMethod { get; set; } // Cash, Insurance, Mixed

        public decimal SubTotal { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;

        public string? Notes { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        // Navigation
        public virtual Patient Patient { get; set; } = null!;
        public virtual Visit? Visit { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
