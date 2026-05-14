using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static MediCare.API.DTOs.BillingDTO;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiillingController : ControllerBase
    {
        private readonly IBillingService _invoiceService;
        private readonly ILogger<BiillingController> _logger;
        public BiillingController(IBillingService billingService, ILogger<BiillingController> logger)
        {
            _invoiceService = billingService;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetAll([FromQuery] InvoiceQueryParams query)
        {
            var result = await _invoiceService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);

            // Patient chỉ xem hóa đơn của mình
            if (User.IsInRole("Patient"))
            {
                var currentUserId = GetCurrentUserId();
                if (invoice.Patient.UserId != currentUserId)
                    return Forbid();
            }

            return Ok(invoice);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _invoiceService.CreateAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id:long}/issue")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Issue(long id)
        {
            var result = await _invoiceService.IssueAsync(id, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/pay")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Pay(long id, [FromBody] PayInvoiceRequest request)
        {
            var result = await _invoiceService.PayAsync(id, request, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(long id)
        {
            var result = await _invoiceService.CancelAsync(id, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPost("{id:long}/items")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> AddItem(long id, [FromBody] AddInvoiceItemRequest request)
        {
            var result = await _invoiceService.AddItemAsync(id, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}/items/{itemId:long}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> UpdateItem(
            long id, long itemId, [FromBody] UpdateInvoiceItemRequest request)
        {
            var result = await _invoiceService.UpdateItemAsync(id, itemId, request);
            return Ok(result);
        }

        [HttpDelete("{id:long}/items/{itemId:long}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeleteItem(long id, long itemId)
        {
            var result = await _invoiceService.DeleteItemAsync(id, itemId);
            return Ok(result);
        }

        /// <summary>Báo cáo doanh thu theo khoảng thời gian</summary>
        [HttpGet("report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate)
        {
            if (fromDate > toDate)
                return BadRequest(new { message = "FromDate không được sau ToDate" });

            var result = await _invoiceService.GetRevenueReportAsync(fromDate, toDate);
            return Ok(result);
        }

        // HELPER 
        private long GetCurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Không xác định được người dùng");

            return userId;
        }
    }
}
