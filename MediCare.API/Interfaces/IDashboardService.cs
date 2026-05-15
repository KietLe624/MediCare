using MediCare.API.DTOs;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardOverviewResponse> GetOverviewAsync();
        Task<List<AppointmentTodayResponse>> GetAppointmentsTodayAsync();
        Task<List<PatientsByDepartmentResponse>> GetPatientsByDepartmentAsync();
        Task<List<VisitsByDateResponse>> GetVisitsByDateAsync(int days); // 7 hoặc 30 ngày
        Task<List<RevenueByDateResponse>> GetRevenueByDateAsync(int days);
        Task<List<RevenueByMonthResponse>> GetRevenueByMonthAsync(int year);

        // Doctor
        Task<DoctorDashboardResponse> GetDoctorDashboardAsync(long doctorId);
    }
}
