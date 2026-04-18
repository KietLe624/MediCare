using AutoMapper;
using MediCare.API.Entities;
using static MediCare.API.DTOs.AuthDTO;

namespace MediCare.API.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequest, ApplicationUser>();

            CreateMap<ApplicationUser, UserInfoResponse>();
        }
    }
}
