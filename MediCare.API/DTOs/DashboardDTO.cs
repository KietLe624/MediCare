namespace MediCare.API.DTOs
{
    public class DashboardOverviewResponse
    {
        public DateOnly Date { get; set; }

        // Bệnh nhân
        public int PatientsToday { get; set; } // lịch hẹn hôm nay
        public int NewPatientsToday { get; set; } // bệnh nhân đăng ký mới hôm nay
        public int TotalPatients { get; set; } // tổng bệnh nhân trong hệ thống

        // Lịch hẹn
        public int AppointmentsToday { get; set; } // tổng lịch hẹn hôm nay
        public int UpcomingAppointments { get; set; } // lịch chưa đến (Scheduled + Confirmed)
        public int CompletedToday { get; set; } // đã hoàn thành hôm nay

        // Doanh thu
        public decimal RevenueToday { get; set; } // doanh thu hôm nay
        public decimal RevenueThisMonth { get; set; } // doanh thu tháng này
        public int PendingInvoices { get; set; } // hóa đơn chưa thanh toán (Draft + Issued)

        // Bác sĩ
        public int DoctorsAvailable { get; set; } // bác sĩ đang available hôm nay
    }
    public class AppointmentTodayResponse
    {
        public long Id { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = default!;
        public string? Reason { get; set; }

        public string PatientName { get; set; } = default!;
        public string PatientUHID { get; set; } = default!;
        public string DoctorName { get; set; } = default!;
        public string DepartmentName { get; set; } = default!;
    }

    // ─── PATIENT STATS ────────────────────────────────────────

    /// <summary>Số bệnh nhân theo khoa</summary>
    public class PatientsByDepartmentResponse
    {
        public string DepartmentName { get; set; } = default!;
        public int Count { get; set; }
    }

    /// <summary>Số lượt khám theo ngày trong 7 hoặc 30 ngày gần nhất</summary>
    public class VisitsByDateResponse
    {
        public DateOnly Date { get; set; }
        public int Count { get; set; }
    }

    // ─── REVENUE STATS ────────────────────────────────────────

    /// <summary>Doanh thu theo ngày</summary>
    public class RevenueByDateResponse
    {
        public DateOnly Date { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>Doanh thu theo tháng trong năm hiện tại</summary>
    public class RevenueByMonthResponse
    {
        public int Month { get; set; } // 1–12
        public string MonthName { get; set; } = default!;
        public decimal Revenue { get; set; }
    }

    // ─── DOCTOR DASHBOARD ─────────────────────────────────────

    /// <summary>Dashboard riêng cho bác sĩ đang đăng nhập</summary>
    public class DoctorDashboardResponse
    {
        public int AppointmentsToday { get; set; }
        public int AppointmentsCompleted { get; set; }
        public int AppointmentsPending { get; set; } // Scheduled + Confirmed còn lại hôm nay
        public int VisitsThisMonth { get; set; }
        public int TotalPatients { get; set; } // tổng bệnh nhân đã khám

        // Lịch hẹn sắp tới hôm nay
        public List<AppointmentTodayResponse> UpcomingToday { get; set; } = new();
    }

    public class DoctorAppointmentStatsResponse
    {
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
    }
}