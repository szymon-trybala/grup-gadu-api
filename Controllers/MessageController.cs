using System.Collections.Generic;
using System.Threading.Tasks;
using grup_gadu_api.DTOs;
using grup_gadu_api.Extensions;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace grup_gadu_api.Controllers
{
  [Authorize]
  public class MessageController : BaseApiController
  {
    private readonly IMessagesService _messagesService;
    public MessageController(IMessagesService messagesService)
    {
      _messagesService = messagesService;
    }

    /// <summary>
    /// Pobiera wszytskie wiadomości z danego czatu
    /// </summary> 
    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> All([FromQuery] int chatId)
    {
      return Ok(await _messagesService.GetAllMessages(User.GetUserId(), chatId));
    }
  }
}