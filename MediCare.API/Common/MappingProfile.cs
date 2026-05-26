using AutoMapper;
using MediCare.API.Entities;
using static MediCare.API.DTOs.AuthDTO;
using static MediCare.API.DTOs.DoctorDTO;
using static MediCare.API.DTOs.UserDTO;
using MediCare.API.DTOs;

namespace MediCare.API.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // AUTH
            CreateMap<RegisterRequest, ApplicationUser>();
            CreateMap<ApplicationUser, UserInfoResponse>();

            // USER
            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<ApplicationUser, UserSummaryResponse>();
            CreateMap<ApplicationUser, UpdateProfileRequest>();
            CreateMap<UpdateProfileRequest, ApplicationUser>();

            // PATIENT
            CreateMap<Patient, PatientResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")) // ghép họ tên
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth))); // tính tuổi từ DateOfBirth

            CreateMap<CreatePatientRequest, Patient>();
            CreateMap<UpdatePatientRequest, Patient>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // chỉ map những trường không null

            CreateMap<Patient, PatientSummaryResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)));

            CreateMap<Patient, PatientLookupResponse>()
                .ForMember(
                    dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.FirstName + " " + src.LastName
                    )
                );

            // DOCTOR
            CreateMap<CreateDoctorRequest, Doctor>();

            CreateMap<Doctor, DoctorSummaryResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName)) // lấy tên đầy đủ từ User
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialization ?? "General"));

            CreateMap<Doctor, DoctorResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName)) // lấy tên đầy đủ từ User
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialization ?? "General"));

            CreateMap<UpdateDoctorRequest, Doctor>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // map những trường không null
            CreateMap<DoctorSchedule, DoctorScheduleResponse>();

            CreateMap<CreateScheduleRequest, DoctorSchedule>();
            CreateMap<UpdateScheduleRequest, DoctorSchedule>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // map những trường không null

            CreateMap<Doctor, DoctorLookupResponse>()
                .ForMember(dest => dest.FullName,opt => opt.MapFrom(src => src.User.FullName));
        }

        // HELPER TÍNH TUỔI
        private static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }
}
