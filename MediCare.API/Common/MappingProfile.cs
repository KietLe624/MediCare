using AutoMapper;
using MediCare.API.Entities;
using static MediCare.API.DTOs.AuthDTO;
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
