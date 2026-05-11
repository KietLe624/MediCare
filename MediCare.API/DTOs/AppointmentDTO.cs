namespace MediCare.API.DTOs
{
    public class AppointmentDTO
    {
        // RESPONSE
        public class AppointmentResponse
        {
            public long Id { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public DateTime AppointmentDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string? Reason { get; set; }
            public string Status { get; set; } = default!;
        }
    }
}
