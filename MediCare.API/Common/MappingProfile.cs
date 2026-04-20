using AutoMapper;
using MediCare.API.Entities;
using static MediCare.API.DTOs.AuthDTO;
using static MediCare.API.DTOs.UserDTO;

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
        }
    }
}
