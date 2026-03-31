using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public long InvoiceId { get; set; }
        public string ItemType { get; set; } = string.Empty; // Consultation, Medication, BedCharge, LabTest, Other
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public long? RefId { get; set; } // Reference to MedicationId, BedId, ...

        // Navigation
        public virtual Invoice Invoice { get; set; } = null!;
    }
}
