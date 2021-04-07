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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessagesService _messagesService;
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IUserRepository _userRepository;
        private static Dictionary<int, List<string>> userGroups = new Dictionary<int, List<string>>();
        private static object _lock = new object();

        public ChatHub(IMessagesService messagesService,
         IChatRepository chatRepository,
         IMapper mapper,
         DataContext context,
         IUserRepository userRepository)
        {
            _messagesService = messagesService;
            _chatRepository = chatRepository;
            _mapper = mapper;
            _context = context;
            _userRepository = userRepository;
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

        public async Task Invite(int userId, int chatId)
        {
          //ToDo: Move this code to service
          Chat chat = await _chatRepository.GetById(chatId);
          AppUser user = await _userRepository.GetUserByIdAsync(userId);
          int adminId = Context.User.GetUserId();

          if (user == null) throw new Exception($"User with id {userId} was not found");
          if (chat == null) throw new Exception($"Chat with id {chatId} was not found");
          if (chat.OwnerId == user.Id) throw new Exception($"User with id {user.Id} is the admin of chat with id {chatId}");
          if (chat.OwnerId != adminId) throw new Exception($"You do not have administrator privileges to add members to the chat");

          UserChats userChat = await _context.UserChats.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == user.Id);
          if (userChat != null) throw new Exception($"User with id {user.Id} is already in the chat with id {chatId}");

          _context.UserChats.Add(new UserChats { ChatId = chatId, UserId = user.Id });
          await _context.SaveChangesAsync();

          ChatDto chatDto = _mapper.Map<ChatDto>(await _chatRepository.GetById(chatId));
          chatDto.UnreadMessages = await _messagesService.CountUnreadMessages(userId, chatId); 
          await Clients.User(userId.ToString()).SendAsync("joinedToNewChat", chatDto);
        }

        public async Task ReadAllMesseges(int chatId)
        {
            await _messagesService.MarkMessagesAsRead(Context.User.GetUserId(), chatId);
        }
    }
}