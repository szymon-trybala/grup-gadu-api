using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using grup_gadu_api.DTOs;
using grup_gadu_api.Extensions;
using grup_gadu_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace grup_gadu_api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessagesService _messagesService;
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private static Dictionary<int, List<string>> userGroups = new Dictionary<int, List<string>>();
        private static object _lock = new object();

        public ChatHub(IMessagesService messagesService, IChatRepository chatRepository, IMapper mapper)
        {
            _messagesService = messagesService;
            _chatRepository = chatRepository;
            _mapper = mapper;
         }

        public async Task Join(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            lock(_lock)
            {
                if (userGroups.ContainsKey(chatId))
                    userGroups[chatId].Add(Context.ConnectionId);
                else
                    userGroups.Add(chatId, new List<string>{Context.ConnectionId});
            }

            ChatDto chat = _mapper.Map<ChatDto>(await _chatRepository.GetById(chatId));
            int userId = Context.User.GetUserId();
            chat.UnreadMessages = await _messagesService.CountUnreadMessages(userId, chatId); 
            await Clients.User(userId.ToString()).SendAsync("joinedToNewChat", chat);
        }

        public async Task Send(int chatId, string message)
        {
            MessageDto msg = await _messagesService.CreateMessage(Context.User.GetUserId(), chatId, message);
            await Clients.Group(chatId.ToString()).SendAsync("newMessage", msg);
            if (userGroups.ContainsKey(chatId))
            await Clients.AllExcept(userGroups[chatId]).SendAsync("newUnread", chatId);
        }
        public async Task Leave(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
            lock(_lock)
            {
                if (userGroups.ContainsKey(chatId))
                    userGroups[chatId].Remove(Context.ConnectionId);
            }
        }

        public async Task ReadAllMesseges(int chatId)
        {
            await _messagesService.MarkMessagesAsRead(Context.User.GetUserId(), chatId);
        }
    }
}