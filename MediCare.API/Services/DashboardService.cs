using MediCare.API.Interfaces;
using MediCare.API.Data;
using MediCare.API.DTOs;
using static MediCare.API.DTOs.UserDTO;
using Microsoft.EntityFrameworkCore;

namespace MediCare.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;
        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // OVERVIEW
        public async Task<DashboardOverviewResponse> GetOverviewAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var thisMonth = new DateOnly(today.Year, today.Month, 1);

            // Convert DayOfWeek to 1–7 (Monday=1, Sunday=7) 
            var dayOfWeek = (int)DateTime.UtcNow.DayOfWeek == 0
                ? 7
                : (int)DateTime.UtcNow.DayOfWeek;

            var patientsToday = await _context.Appointments
                .Where(a => a.AppointmentDate == today)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            var newPatientsToday = await _context.Patients
                .Where(p => DateOnly.FromDateTime(p.CreatedAt) == today)
                .CountAsync();

            var totalPatients = await _context.Patients.CountAsync();

            var appointmentsToday = await _context.Appointments
                .Where(a => a.AppointmentDate == today)
                .CountAsync();

            var upcomingAppointments = await _context.Appointments
                .Where(a =>
                    a.AppointmentDate >= today &&
                    (a.Status == "Scheduled" || a.Status == "Confirmed"))
                .CountAsync();

            var completedToday = await _context.Appointments
                .Where(a =>
                    a.AppointmentDate == today &&
                    a.Status == "Completed")
                .CountAsync();

            var revenueToday = await _context.Invoices
                .Where(i =>
                    DateOnly.FromDateTime(i.CreatedAt) == today &&
                    (i.Status == "Paid" || i.Status == "PartialPaid"))
                .SumAsync(i => (decimal?)i.PaidAmount) ?? 0; // nullable để tránh lỗi khi không có data

            var revenueThisMonth = await _context.Invoices
                .Where(i =>
                    i.CreatedAt.Year == today.Year &&
                    i.CreatedAt.Month == today.Month &&
                    (i.Status == "Paid" || i.Status == "PartialPaid"))
                .SumAsync(i => (decimal?)i.PaidAmount) ?? 0;

            var pendingInvoices = await _context.Invoices
                .Where(i => i.Status == "Draft" || i.Status == "Issued")
                .CountAsync();
            var doctorsAvailable = await _context.Doctors
                .Where(d =>
                    d.IsAvailable &&
                    d.Schedules.Any(s => s.IsActive && s.DayOfWeek == dayOfWeek))
                .CountAsync();

            return new DashboardOverviewResponse
            {
                Date = today,
                PatientsToday = patientsToday,
                NewPatientsToday = newPatientsToday,
                TotalPatients = totalPatients,
                AppointmentsToday = appointmentsToday,
                UpcomingAppointments = upcomingAppointments,
                CompletedToday = completedToday,
                RevenueToday = revenueToday,
                RevenueThisMonth = revenueThisMonth,
                PendingInvoices = pendingInvoices,
                DoctorsAvailable = doctorsAvailable
            };
        }

        // APPPOINTMENTS TODAY 
        public async Task<List<AppointmentTodayResponse>> GetAppointmentsTodayAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);


            /* Lấy thông tin lịch hẹn bao gồm:
             * Patient: tên, UHID
             * Doctor : tên, khoa
             * Department: tên
             * Thời gian: bắt đầu, kết thúc
             */
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentDate == today)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return appointments.Select(a => new AppointmentTodayResponse
            {
                Id = a.Id,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason,
                PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                PatientUHID = a.Patient.UHID,
                DoctorName = a.Doctor.User.FullName,
                DepartmentName = a.Doctor.Department.Name
            }).ToList();
        }

        // PATIENTS BY DEPARTMENT
        public async Task<List<PatientsByDepartmentResponse>> GetPatientsByDepartmentAsync()
        {
            // Đếm số bệnh nhân unique theo khoa (qua appointment)
            var result = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .GroupBy(a => a.Doctor.Department.Name)
                .Select(g => new PatientsByDepartmentResponse
                {
                    DepartmentName = g.Key,
                    Count = g.Select(a => a.PatientId).Distinct().Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return result;
        }

        // VISITS BY DATE
        public async Task<List<VisitsByDateResponse>> GetVisitsByDateAsync(int days)
        {
            // Giới hạn days hợp lệ: 7 hoặc 30
            days = days <= 7 ? 7 : 30;

            // Tính khoảng thời gian từ ngày hiện tại trở về trước
            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days + 1));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Lấy số lượt khám theo ngày từ DB
            var visitCounts = await _context.Visits
                .AsNoTracking()
                .Where(v =>
                    DateOnly.FromDateTime(v.VisitDate) >= fromDate &&
                    DateOnly.FromDateTime(v.VisitDate) <= today)
                .GroupBy(v => DateOnly.FromDateTime(v.VisitDate))
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            var result = new List<VisitsByDateResponse>();
            for (var date = fromDate; date <= today; date = date.AddDays(1))
            {
                result.Add(new VisitsByDateResponse
                {
                    Date = date,
                    Count = visitCounts.GetValueOrDefault(date, 0) // lượt khám
                });
            }

            return result;
        }

        // REVENUE BY DATE
        public async Task<List<RevenueByDateResponse>> GetRevenueByDateAsync(int days)
        {
            days = days <= 7 ? 7 : 30;

            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days + 1));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var revenueCounts = await _context.Invoices
                .AsNoTracking()
                .Where(i =>
                    DateOnly.FromDateTime(i.CreatedAt) >= fromDate &&
                    DateOnly.FromDateTime(i.CreatedAt) <= today &&
                    (i.Status == "Paid" || i.Status == "PartialPaid"))
                .GroupBy(i => DateOnly.FromDateTime(i.CreatedAt))
                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.PaidAmount) })
                .ToDictionaryAsync(x => x.Date, x => x.Revenue);

            var result = new List<RevenueByDateResponse>();
            for (var date = fromDate; date <= today; date = date.AddDays(1))
            {
                result.Add(new RevenueByDateResponse
                {
                    Date = date,
                    Revenue = revenueCounts.GetValueOrDefault(date, 0)
                });
            }

            return result;
        }

        // REVENUE BY MONTH

        public async Task<List<RevenueByMonthResponse>> GetRevenueByMonthAsync(int year)
        {
            var revenueCounts = await _context.Invoices
                .AsNoTracking()
                .Where(i =>
                    i.CreatedAt.Year == year &&
                    (i.Status == "Paid" || i.Status == "PartialPaid"))
                .GroupBy(i => i.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.PaidAmount) })
                .ToDictionaryAsync(x => x.Month, x => x.Revenue);

            var monthNames = new[]
            {
                "", "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4",
                "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8",
                "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
            };

            return Enumerable.Range(1, 12).Select(month => new RevenueByMonthResponse
            {
                Month = month,
                MonthName = monthNames[month],
                Revenue = revenueCounts.GetValueOrDefault(month, 0)
            }).ToList();
        }

        // DOCTOR DASHBOARD
        public async Task<DoctorDashboardResponse> GetDoctorDashboardAsync(long doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var thisMonth = new DateOnly(today.Year, today.Month, 1);

            // Lịch hẹn hôm nay của bác sĩ
            var todayAppointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate == today)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            // Số lượt khám trong tháng
            var visitsThisMonth = await _context.Visits
                .Where(v =>
                    v.Appointment.DoctorId == doctorId &&
                    DateOnly.FromDateTime(v.VisitDate) >= thisMonth)
                .CountAsync();

            // Tổng bệnh nhân đã từng khám với bác sĩ này
            var totalPatients = await _context.Appointments
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.Status == "Completed")
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            var completed = todayAppointments.Count(a => a.Status == "Completed");
            var pending = todayAppointments.Count(a =>
                a.Status == "Scheduled" || a.Status == "Confirmed");

            // Lịch hẹn sắp tới — chưa hoàn thành hôm nay
            var upcoming = todayAppointments
                .Where(a => a.Status == "Scheduled" || a.Status == "Confirmed")
                .Select(a => new AppointmentTodayResponse
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Reason = a.Reason,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PatientUHID = a.Patient.UHID,
                    DoctorName = a.Doctor.User.FullName,
                    DepartmentName = a.Doctor.Department.Name
                }).ToList();

            return new DoctorDashboardResponse
            {
                AppointmentsToday = todayAppointments.Count,
                AppointmentsCompleted = completed,
                AppointmentsPending = pending,
                VisitsThisMonth = visitsThisMonth,
                TotalPatients = totalPatients,
                UpcomingToday = upcoming
            };
        }

        // DOCTOR APPOINTMENTS BY DATE
        public async Task<List<DoctorAppointmentStatsResponse>> GetDoctorAppointmentsByDateAsync(int days)
        {
            days = days <= 7 ? 7 : 30;
            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days + 1));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.AppointmentDate >= fromDate &&
                a.AppointmentDate <= today)
            .GroupBy(a => new
            {
                a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                DepartmentName = a.Doctor.Department.Name
            })
            .Select(g => new DoctorAppointmentStatsResponse
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName,
                DepartmentName = g.Key.DepartmentName,
                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.AppointmentCount)
            .ToListAsync();
            return result;
        }

    }

}
