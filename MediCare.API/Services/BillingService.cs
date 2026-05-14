using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.BillingDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class BillingService : IBillingService
    { 
        private readonly ILogger<BillingService> _logger;
        private readonly AppDbContext _context;
        public BillingService(ILogger<BillingService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET ALL
        public async Task<PagedResponse<InvoiceSummaryResponse>> GetAllAsync(InvoiceQueryParams query)
        {
            var q = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .AsQueryable();

            // FILTER
            if (query.PatientId.HasValue)
                q = q.Where(i => i.PatientId == query.PatientId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(i => i.Status == query.Status);

            if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
                q = q.Where(i => i.PaymentMethod == query.PaymentMethod);

            if (query.FromDate.HasValue)
                q = q.Where(i => DateOnly.FromDateTime(i.CreatedAt) >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(i => DateOnly.FromDateTime(i.CreatedAt) <= query.ToDate.Value);

            // Sort theo ngày tạo
            q = query.SortOrder == "asc"
                ? q.OrderBy(i => i.CreatedAt)
                : q.OrderByDescending(i => i.CreatedAt);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var invoices = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<InvoiceSummaryResponse>
            {
                Data = invoices.Select(MapToSummaryResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // GET BY ID
        public async Task<InvoiceResponse> GetByIdAsync(long id)
        {
            var invoice = await FindInvoiceOrThrowAsync(id);
            return MapToResponse(invoice);
        }

        // CREATE
        public async Task<InvoiceResponse> CreateAsync(
            CreateInvoiceRequest request, long createdByUserId)
        {
            // Kiểm tra Patient 
            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId);
            if (!patientExists)
                throw new BadHttpRequestException(
                    $"Không tìm thấy bệnh nhân với ID {request.PatientId}");

            // Kiểm tra Visit 
            if (request.VisitId.HasValue)
            {
                var visitExists = await _context.Visits
                    .AnyAsync(v => v.Id == request.VisitId.Value);
                if (!visitExists)
                    throw new BadHttpRequestException(
                        $"Không tìm thấy bệnh án với ID {request.VisitId}");
            }

            // Tính SubTotal từ các items
            var subTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var total = subTotal - request.DiscountAmount + request.TaxAmount;

            // Đảm bảo total không âm
            if (total < 0)
                throw new BadHttpRequestException("Tổng hóa đơn không thể âm");

            // Tạo hóa đơn mới
            var invoice = new Invoice
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                PatientId = request.PatientId,
                VisitId = request.VisitId,
                Status = "Draft",
                PaymentMethod = request.PaymentMethod,
                DiscountAmount = request.DiscountAmount,
                TaxAmount = request.TaxAmount,
                SubTotal = subTotal,
                TotalAmount = total,
                PaidAmount = 0,
                Notes = request.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                // Tạo các dòng item
                InvoiceItems = request.Items.Select(i => new InvoiceItem
                {
                    ItemType = i.ItemType,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice,
                    RefId = i.RefId
                }).ToList()
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Invoice created: Id={Id}, Number={Number}, Patient={PatientId}, Total={Total}",
                invoice.Id, invoice.InvoiceNumber, invoice.PatientId, invoice.TotalAmount);

            return await GetByIdAsync(invoice.Id);
        }

        // CHANGE STATUS
        public async Task<InvoiceResponse> IssueAsync(long id, long updatedByUserId)
        {
            var invoice = await FindInvoiceOrThrowAsync(id);

            // Chỉ phát hành từ Draft
            if (invoice.Status != "Draft")
                throw new BadHttpRequestException(
                    $"Không thể phát hành hóa đơn đang ở trạng thái '{invoice.Status}'");

            // Phải có ít nhất 1 item
            if (!invoice.InvoiceItems.Any())
                throw new BadHttpRequestException("Hóa đơn phải có ít nhất 1 dòng trước khi phát hành");

            invoice.Status = "Issued"; // Cập nhật trạng thái
            invoice.IssuedAt = DateTime.UtcNow;
            invoice.UpdatedBy = updatedByUserId;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Invoice {Id} issued", id);

            return MapToResponse(invoice);
        }

        // PAY
        public async Task<InvoiceResponse> PayAsync(
            long id, PayInvoiceRequest request, long updatedByUserId)
        {
            var invoice = await FindInvoiceOrThrowAsync(id);

            // Chỉ thanh toán khi đang Issued hoặc PartialPaid
            if (invoice.Status != "Issued" && invoice.Status != "PartialPaid")
                throw new BadHttpRequestException(
                    $"Không thể thanh toán hóa đơn đang ở trạng thái '{invoice.Status}'");

            var remaining = invoice.TotalAmount - invoice.PaidAmount;

            // Số tiền thanh toán không được vượt quá số còn nợ
            if (request.PaidAmount > remaining)
                throw new BadHttpRequestException(
                    $"Số tiền thanh toán ({request.PaidAmount:N0}) vượt quá số còn lại ({remaining:N0})");

            invoice.PaidAmount += request.PaidAmount;

            // Override PaymentMethod nếu có
            if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
                invoice.PaymentMethod = request.PaymentMethod;

            // Xác định trạng thái: Paid nếu đủ, PartialPaid nếu chưa đủ
            invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? "Paid" : "PartialPaid";

            if (invoice.Status == "Paid")
                invoice.PaidAt = DateTime.UtcNow;

            invoice.UpdatedBy = updatedByUserId;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Invoice {Id} payment: +{Amount}, Status={Status}, Total paid={Total}",
                id, request.PaidAmount, invoice.Status, invoice.PaidAmount);

            return MapToResponse(invoice);
        }

        // CANCEL
        public async Task<InvoiceResponse> CancelAsync(long id, long updatedByUserId)
        {
            var invoice = await FindInvoiceOrThrowAsync(id);

            // Chỉ hủy được Draft hoặc Issued
            if (invoice.Status != "Draft" && invoice.Status != "Issued")
                throw new BadHttpRequestException(
                    $"Không thể hủy hóa đơn đang ở trạng thái '{invoice.Status}'");

            // Nếu đã có thanh toán một phần → không cho hủy, phải refund trước
            if (invoice.PaidAmount > 0)
                throw new BadHttpRequestException(
                    "Không thể hủy hóa đơn đã có thanh toán. Vui lòng liên hệ admin để xử lý hoàn tiền.");

            invoice.Status = "Cancelled";
            invoice.UpdatedBy = updatedByUserId;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogWarning("Invoice {Id} cancelled", id);

            return MapToResponse(invoice);
        }

        // INVOICE ITEMS
        public async Task<InvoiceResponse> AddItemAsync(
            long invoiceId, AddInvoiceItemRequest request)
        {
            var invoice = await FindInvoiceOrThrowAsync(invoiceId);

            // Chỉ sửa items khi còn Draft
            EnsureInvoiceIsDraft(invoice);

            var item = new InvoiceItem
            {
                InvoiceId = invoiceId,
                ItemType = request.ItemType,
                Description = request.Description,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                TotalPrice = request.Quantity * request.UnitPrice,
                RefId = request.RefId
            };

            _context.InvoiceItems.Add(item);

            // Tính lại SubTotal và TotalAmount
            RecalculateTotals(invoice, item.TotalPrice);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(invoiceId);
        }

        public async Task<InvoiceResponse> UpdateItemAsync(
           long invoiceId, long itemId, UpdateInvoiceItemRequest request)
        {
            var invoice = await FindInvoiceOrThrowAsync(invoiceId);
            EnsureInvoiceIsDraft(invoice);

            var item = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException(
                    $"Không tìm thấy dòng hóa đơn ID {itemId}");

            // Cập nhật và tính lại tổng
            var oldTotal = item.TotalPrice;
            item.Description = request.Description;
            item.Quantity = request.Quantity;
            item.UnitPrice = request.UnitPrice;
            item.TotalPrice = request.Quantity * request.UnitPrice;

            var diff = item.TotalPrice - oldTotal;
            RecalculateTotals(invoice, diff);

            await _context.SaveChangesAsync();

            return MapToResponse(invoice);
        }

        public async Task<InvoiceResponse> DeleteItemAsync(long invoiceId, long itemId)
        {
            var invoice = await FindInvoiceOrThrowAsync(invoiceId);
            EnsureInvoiceIsDraft(invoice);

            // Phải có ít nhất 1 item sau khi xóa
            if (invoice.InvoiceItems.Count <= 1)
                throw new BadHttpRequestException(
                    "Hóa đơn phải có ít nhất 1 dòng. Không thể xóa dòng cuối cùng.");

            var item = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException(
                    $"Không tìm thấy dòng hóa đơn ID {itemId}");

            _context.InvoiceItems.Remove(item);
            RecalculateTotals(invoice, -item.TotalPrice);

            await _context.SaveChangesAsync();

            return MapToResponse(invoice);
        }

        // REPORT
        public async Task<RevenueReportResponse> GetRevenueReportAsync(
            DateOnly fromDate, DateOnly toDate)
        {
            var invoices = await _context.Invoices
                .AsNoTracking()
                .Where(i =>
                    DateOnly.FromDateTime(i.CreatedAt) >= fromDate &&
                    DateOnly.FromDateTime(i.CreatedAt) <= toDate &&
                    i.Status != "Cancelled")
                .ToListAsync();

            // Chỉ tính doanh thu từ hóa đơn đã thu tiền
            var paidInvoices = invoices
                .Where(i => i.Status == "Paid" || i.Status == "PartialPaid")
                .ToList();

            return new RevenueReportResponse
            {
                TotalRevenue = paidInvoices.Sum(i => i.PaidAmount),
                TotalPaid = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount),
                TotalRemaining = invoices.Sum(i => i.TotalAmount - i.PaidAmount),
                TotalInvoices = invoices.Count,
                PaidCount = invoices.Count(i => i.Status == "Paid"),
                PartialPaidCount = invoices.Count(i => i.Status == "PartialPaid"),
                PendingCount = invoices.Count(i => i.Status == "Draft" || i.Status == "Issued"),
                CashRevenue = paidInvoices.Where(i => i.PaymentMethod == "Cash")
                                              .Sum(i => i.PaidAmount),
                InsuranceRevenue = paidInvoices.Where(i => i.PaymentMethod == "Insurance")
                                               .Sum(i => i.PaidAmount),
                MixedRevenue = paidInvoices.Where(i => i.PaymentMethod == "Mixed")
                                              .Sum(i => i.PaidAmount)
            };
        }

        // HELPER
        private async Task<Invoice> FindInvoiceOrThrowAsync(long id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                throw new KeyNotFoundException($"Không tìm thấy hóa đơn với ID {id}");

            return invoice;
        }
        private static void EnsureInvoiceIsDraft(Invoice invoice)
        {
            if (invoice.Status != "Draft")
                throw new BadHttpRequestException(
                    $"Chỉ có thể chỉnh sửa hóa đơn khi đang ở trạng thái bản nháp(Draft). " +
                    $"Hiện tại: '{invoice.Status}'");
        }
        /// <summary>
        /// Tính lại SubTotal và TotalAmount khi thêm/sửa/xóa item.
        /// TotalAmount = SubTotal - Discount + Tax
        /// </summary>
        private static void RecalculateTotals(Invoice invoice, decimal itemTotalDiff)
        {
            invoice.SubTotal += itemTotalDiff;
            invoice.TotalAmount = invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount;
        }

        /// <summary>
        /// Sinh số hóa đơn: INV-YYYYMMDD-{4 số thứ tự trong ngày}
        /// Ví dụ: INV-20250410-0001
        /// </summary>
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"INV-{today}-";

            var count = await _context.Invoices
                .CountAsync(i => i.InvoiceNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }

        private static InvoiceResponse MapToResponse(Invoice i) => new()
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Status = i.Status,
            PaymentMethod = i.PaymentMethod,
            SubTotal = i.SubTotal,
            DiscountAmount = i.DiscountAmount,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            RemainingAmount = i.TotalAmount - i.PaidAmount,
            Notes = i.Notes,
            IssuedAt = i.IssuedAt,
            PaidAt = i.PaidAt,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
            Patient = new PatientBriefResponse
            {
                Id = i.Patient.Id,
                UHID = i.Patient.UHID,
                FullName = $"{i.Patient.FirstName} {i.Patient.LastName}",
                UserId = i.Patient.UserId
            },
            Items = i.InvoiceItems
                .OrderBy(item => item.Id)
                .Select(item => new InvoiceItemResponse
                {
                    Id = item.Id,
                    ItemType = item.ItemType,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    RefId = item.RefId
                }).ToList()
        };
        private static InvoiceSummaryResponse MapToSummaryResponse(Invoice i) => new()
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Status = i.Status,
            PaymentMethod = i.PaymentMethod,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            RemainingAmount = i.TotalAmount - i.PaidAmount,
            IssuedAt = i.IssuedAt,
            CreatedAt = i.CreatedAt,
            Patient = new PatientBriefResponse
            {
                Id = i.Patient.Id,
                UHID = i.Patient.UHID,
                FullName = $"{i.Patient.FirstName} {i.Patient.LastName}",
                UserId = i.Patient.UserId
            }
        };
    }
}
