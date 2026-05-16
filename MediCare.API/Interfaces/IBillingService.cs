using static MediCare.API.DTOs.BillingDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IBillingService
    {
        // ─── CRUD ─────────────────────────────────────────────
        Task<PagedResponse<InvoiceSummaryResponse>> GetAllAsync(InvoiceQueryParams query);
        Task<InvoiceResponse> GetByIdAsync(long id);
        Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, long createdByUserId);

        // ─── STATUS TRANSITIONS ───────────────────────────────
        Task<InvoiceResponse> IssueAsync(long id, long updatedByUserId);                    // Draft → Issued
        Task<InvoiceResponse> PayAsync(long id, PayInvoiceRequest request, long updatedByUserId);  // Issued → Paid / PartialPaid
        Task<InvoiceResponse> CancelAsync(long id, long updatedByUserId);                   // Draft / Issued → Cancelled

        // ─── INVOICE ITEMS ────────────────────────────────────
        Task<InvoiceResponse> AddItemAsync(long invoiceId, AddInvoiceItemRequest request);
        Task<InvoiceResponse> UpdateItemAsync(long invoiceId, long itemId, UpdateInvoiceItemRequest request);
        Task<InvoiceResponse> DeleteItemAsync(long invoiceId, long itemId);

        // ─── REPORTS ──────────────────────────────────────────
        Task<RevenueReportResponse> GetRevenueReportAsync(DateOnly fromDate, DateOnly toDate);
    }
}
