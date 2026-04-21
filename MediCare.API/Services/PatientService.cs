using AutoMapper;
using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.PatientDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class PatientService : IPatientService
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(AppDbContext context, IMapper mapper, ILogger<PatientService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientQueryParams query)
        {
            // lấy tất cả bệnh nhân
            var patientsQuery = _context.Patients.AsNoTracking();

            // filter(tên, UHID, SĐT)
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchLower = query.Search.ToLower(); // convert sang chữ thường
                patientsQuery = patientsQuery.Where(p =>
                    p.FirstName.ToLower().Contains(searchLower) ||
                    p.LastName.ToLower().Contains(searchLower) ||
                    p.UHID.ToLower().Contains(searchLower) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchLower))
                );
            }

            // filter loại bệnh nhân (inpatient, outpatient)
            if (!string.IsNullOrEmpty(query.PatientType))
            {
                patientsQuery = patientsQuery.Where(p => p.PatientType == query.PatientType);
            }

            // filter nhóm máu
            if (!string.IsNullOrEmpty(query.BloodType))
            {
                patientsQuery = patientsQuery.Where(p => p.BloodType == query.BloodType);
            }

            var totalCount = await patientsQuery.CountAsync();

            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);
            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<PatientSummaryResponse>>(patients); // map sang DTO trả về

            return new PagedResponse<PatientSummaryResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        public Task<PatientResponse> CreateAsync(CreatePatientRequest request, long createdByUserId)
        {
            if (request.UserId.HasValue)
            {
                // kiểm tra nếu có UserId thì phải tồn tại user đó
                var userExists = _context.Users.Any(u => u.Id == request.UserId.Value);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"Không tìm thấy user với ID {request.UserId.Value}");
                }
                var userAlreadyPatient = _context.Patients.Any(p => p.UserId == request.UserId.Value);
                if (userAlreadyPatient)
                {
                    throw new InvalidOperationException($"User với ID {request.UserId.Value} đã liên kết với một hồ sơ khác");
                }
                
            }
            var patient = _mapper.Map<Patient>(request);
            patient.UHID = GenerateUHIDAsync().GetAwaiter().GetResult(); // sinh UHID
            patient.CreatedBy = createdByUserId;
            patient.CreatedAt = DateTime.UtcNow;
            _context.Patients.Add(patient); // thêm vào DbContext
            _context.SaveChanges(); // lưu vào database

            _logger.LogInformation(
            "Tạo thành công hồ sơ bệnh nhân: UHID={UHID}, Id={Id}, By={UserId}",
            patient.UHID, patient.Id, createdByUserId);

            return Task.FromResult(_mapper.Map<PatientResponse>(patient));
        }

        public async Task<PatientResponse> GetByIdAsync(long id)
        {
            var patient = await FindPatientOrThrowAsync(id);

            // sử dụng AutoMapper để map từ entity sang DTO trả về
            return _mapper.Map<PatientResponse>(patient);
        }

        public async Task<PatientResponse> UpdateAsync(long id, UpdatePatientRequest request, long updatedByUserId)
        {
            // tìm bệnh nhân, nếu không có sẽ ném lỗi
            var patient = await FindPatientOrThrowAsync(id);
            _mapper.Map(request, patient);
            patient.UpdatedBy = updatedByUserId;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Patient updated: Id={Id}, By={UserId}", id, updatedByUserId);

            return _mapper.Map<PatientResponse>(patient);
        }

        // HELPER
        private async Task<string> GenerateUHIDAsync()
        {
            /// <summary>
            /// Sinh UHID: PT-YYYYMMDD-{4 số thứ tự trong ngày}
            /// Ví dụ: PT-20250410-0001
            /// </summary>
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"PT-{today}-";

            // Đếm số bệnh nhân được tạo trong ngày để sinh số thứ tự
            var countToday = await _context.Patients
                .CountAsync(p => p.UHID.StartsWith(prefix));

            return $"{prefix}{(countToday + 1):D4}";
        }

        private async Task<Patient> FindPatientOrThrowAsync(long id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bệnh nhân với ID {id}");
            }
            return patient;
        }
    }
}
