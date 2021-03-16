using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using grup_gadu_api.Data;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;
using grup_gadu_api.Extensions;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace grup_gadu_api.Controllers
{
  [Authorize]
  public class ChatController : BaseApiController
  {
    public readonly DataContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly IMapper _mapper;

    public ChatController(DataContext context, IChatRepository chatRepository, IMapper mapper)
    {
      _mapper = mapper;
      _chatRepository = chatRepository;
      _context = context;
    }

   
    /// <summary>
    /// Tworzy nowy czat
    /// </summary>
    /// <param name="name"></param>    
    [HttpPost("[action]")]
    public async Task<ActionResult<ChatDto>> Create([FromQuery] string name)
    {
      Chat chat = new Chat
      {
        Name = name,
        CreatedAt = DateTime.Now,
        IsActive = true,
        OwnerId = User.GetUserId()
      };

      _context.Chats.Add(chat);
      await _context.SaveChangesAsync();

      return new ChatDto
      {
        CreatedAt = chat.CreatedAt,
        Id = chat.Id,
        Members = new List<string>(),
        Name = name,
        OwnerLogin = User.GetLogin()
      };
    }

   
    /// <summary>
    /// Zwraca liste czatow do ktorych nalezy user lub ktore adminuje
    /// </summary> 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatDto>>> List()
    {
      IEnumerable<Chat> chats = await this._chatRepository.GetChatsAsync(User.GetUserId());
      return Ok(_mapper.Map<IEnumerable<ChatDto>>(chats));
    }

  }
}