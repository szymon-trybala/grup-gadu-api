using System.Collections.Generic;
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
            .ForMember(x => x.Owner, opt => opt.MapFrom(src => new MemberDto {Id = src.OwnerId, Login = src.Owner.Login }))
            .ForMember(x => x.Members, opt => opt.MapFrom(src => new List<MemberDto>(src.Members.Select(x => new MemberDto {Login = x.User.Login, Id = x.UserId}))));

          CreateMap<Message, MessageDto>()
            .ForMember(x => x.AuthorLogin, opt => opt.MapFrom(src => src.Author.Login))
            .ForMember(x => x.ChatName, opt => opt.MapFrom(src => src.Chat.Name));

          CreateMap<UserMessages, SeenByDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(x => x.Login, opt => opt.MapFrom(src => src.User.Login));
        }
    }
}