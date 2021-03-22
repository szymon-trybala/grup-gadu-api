using System.Collections.Generic;
using System.Threading.Tasks;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> List([FromQuery] int chatId)
    {
      return Ok(await _messagesService.GetMessages(chatId));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromQuery]int chatId, [FromQuery]string messageContent)
    {
      await _messagesService.CreateMessage(User.GetUserId(), chatId, messageContent);
      return Ok();
    }
  }
}