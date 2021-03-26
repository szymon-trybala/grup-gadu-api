using System.Threading.Tasks;
using grup_gadu_api.DTOs;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace grup_gadu_api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessagesService _messagesService;

        public ChatHub(IMessagesService messagesService)
        {
          _messagesService = messagesService;
        }

        public async Task Join(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task Send(int userId, int chatId, string message)
        {
            MessageDto msg = await _messagesService.CreateMessage(userId, chatId, message);
            await Clients.Group(chatId.ToString()).SendAsync("newMessage", msg);
        }

        public async Task Leave(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }
    }
}