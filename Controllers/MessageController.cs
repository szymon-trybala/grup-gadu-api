using System.Collections.Generic;
using System.Threading.Tasks;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;
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

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> All([FromQuery] int chatId)
    {
      return Ok(await _messagesService.GetAllMessages(User.GetUserId(), chatId));
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> Unread([FromQuery] int chatId)
    {
      return Ok(await _messagesService.GetUnreadMessages(User.GetUserId(),chatId));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromQuery]int chatId, [FromQuery]string messageContent)
    {
      await _messagesService.CreateMessage(User.GetUserId(), chatId, messageContent);
      return Ok();
    }
  }
}