using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.UserDTO;


namespace MediCare.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<AuditLogResponse>> GetAllAsync(AuditLogQueryParams query)
        {
            var q = _context.AuditLogs.AsNoTracking();

            if (query.UserId.HasValue)
                q = q.Where(a => a.UserId == query.UserId.Value);

            if (!string.IsNullOrWhiteSpace(query.Action))
                q = q.Where(a => a.Action == query.Action);

            if (!string.IsNullOrWhiteSpace(query.EntityName))
                q = q.Where(a => a.EntityName == query.EntityName);

            if (query.FromDate.HasValue)
                q = q.Where(a => DateOnly.FromDateTime(a.CreatedAt) >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(a => DateOnly.FromDateTime(a.CreatedAt) <= query.ToDate.Value);

            // Mới nhất lên đầu
            q = q.OrderByDescending(a => a.CreatedAt);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var logs = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<AuditLogResponse>
            {
                Data = logs.Select(a => new AuditLogResponse
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    CreatedAt = a.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AuditLogResponse> GetByIdAsync(long id)
        {
            var log = await _context.AuditLogs.FindAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy audit log với ID {id}");

            return new AuditLogResponse
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            };
        }
    }
}
