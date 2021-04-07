using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using grup_gadu_api.Data;
using grup_gadu_api.DTOs;
using grup_gadu_api.Extensions;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Controllers
{
  [Authorize]
  public class MessageController : BaseApiController
  {
    private readonly IMessagesService _messagesService;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageController(
    IMessagesService messagesService,
    DataContext context,
    IMapper mapper)
    {
      _messagesService = messagesService;
      _context = context;
      _mapper = mapper;
    }

    /// <summary>
    /// Pobiera wszytskie wiadomości z danego czatu
    /// </summary> 
    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> All([FromQuery] int chatId)
    {
      return Ok(await _messagesService.GetAllMessages(User.GetUserId(), chatId));
    }

    /// <summary>
    /// Pobiera informację o tym kiedy i prze kogo dana wiadomośc została przeczytana
    /// </summary> 
    [HttpGet("[action]")]
    public async Task<ActionResult<List<SeenByDto>>> Details([FromQuery] int chatId, [FromQuery] int messageId)
    {
      if(!await _messagesService.HasPermissionToRead(User.GetUserId(), chatId)) 
        return Unauthorized($"User does not have read messages from chat with id:{chatId}");

      if((await _context.Messages.FindAsync(messageId)).ChatId != chatId)
        return BadRequest($"Message with id {messageId} does not belog to chat with id {chatId}");

       var seenBy = await _context.UserMessages
                          .Include(x => x.User)
                          .Where(x=> x.MessageId == messageId)
                          .ToListAsync();
                          
       return Ok(_mapper.Map<IEnumerable<SeenByDto>>(seenBy));
    }
  }
}