using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using grup_gadu_api.Data;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;
using grup_gadu_api.Extensions;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Controllers
{
  [Authorize]
  public class ChatController : BaseApiController
  {
    public readonly DataContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public ChatController(DataContext context, IChatRepository chatRepository, IMapper mapper, IUserRepository userRepository)
    {
      _userRepository = userRepository;
      _mapper = mapper;
      _chatRepository = chatRepository;
      _context = context;
    }

    /// <summary>
    /// Tworzy nowy czat
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ActionResult<ChatDto>> Create([FromQuery] string name)
    {
      string chatName = name.ToLower();
      if(await _chatRepository.GetByName(chatName) != null) 
          return BadRequest($"Chat with name {chatName} already exists");

        Chat chat = new Chat
        {
          Name = chatName,
          CreatedAt = DateTime.Now,
          IsActive = true,
          OwnerId = User.GetUserId()
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

         return Created("",_mapper.Map<ChatDto>(await _chatRepository.GetById(chat.Id)));
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

    /// <summary>
    /// Dodaje usera do danego czatu
    /// </summary> 
    [HttpPost("[action]")]
    public async Task<ActionResult> Invite([FromQuery] string userLogin, [FromQuery] int chatId)
    {
      Chat chat = await _chatRepository.GetById(chatId);
      AppUser user = await _userRepository.GetUserByLoginAsync(userLogin);
      
      if (user == null)  return NotFound($"User with login {userLogin} was not found");
      if (chat == null) return NotFound($"Chat with id {chatId} was not found");
      if (chat.OwnerId == user.Id) return BadRequest($"User with id {user.Id} is the admin of chat with id {chatId}");
      if (chat.OwnerId == User.GetUserId()) return BadRequest($"You do not have administrator privileges to add members to the chat");

      UserChats userChat = await _context.UserChats.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == user.Id);
      if (userChat != null) return BadRequest($"User with id {user.Id} is already in the chat with id {chatId}");

      _context.UserChats.Add(new UserChats { ChatId = chatId, UserId = user.Id });
      await _context.SaveChangesAsync();

      return Ok();
    }

    /// <summary>
    /// Usuwa aktualnie zalogowanego usera z danego czatu
    /// </summary> 
    [HttpPost("[action]")]
    public async Task<ActionResult> Leave([FromQuery] int chatId)
    {
      Chat chat = await _chatRepository.GetById(chatId);
      if (chat == null) return BadRequest($"Chat with id {chatId} was not found");
      if(chat.OwnerId == User.GetUserId())
      {
        if(chat.Members.Any()) return BadRequest($"You cannot leave your own chat until there are other chat members");
      }

      UserChats userChat = await _context.UserChats.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == User.GetUserId());
      if (userChat == null) return BadRequest($"User with id {User.GetUserId()} does not belong to chat with id {chatId}");

      _context.UserChats.Remove(userChat);
      await _context.SaveChangesAsync();

      return Ok();
    }

  }
}