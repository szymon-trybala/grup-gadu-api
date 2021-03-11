using AutoMapper;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
           CreateMap<RegisterDto, AppUser>();
        }
    }
}