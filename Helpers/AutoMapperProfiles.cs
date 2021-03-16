using System.Linq;
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
           CreateMap<Chat, ChatDto>()
            .ForMember(x => x.OwnerLogin, opt => opt.MapFrom(src => src.Owner.Login))
            .ForMember(x => x.Members, opt => opt.MapFrom(src => src.Members.Select(x => x.User.Login)));
        }
    }
}