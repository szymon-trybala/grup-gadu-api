using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using grup_gadu_api.Data;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;
using grup_gadu_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Services
{
  public class MessagesService : IMessagesService
  {
    private static ConcurrentDictionary<int, ReaderWriterLockSlim> locks = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessagesService(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<int> CountUnreadMessages(int userId, int chatId)
    {
      return await _context.Messages
           .Include(x=> x.SeenBy)
           .Where(x => x.ChatId == chatId)
           .Where(x => x.AuthorId != userId)
           .Where(x=> !x.SeenBy.Any(y=> y.MessageId == x.Id && y.UserId == userId))
           .CountAsync();
    }

    public async Task<MessageDto>CreateMessage(int userId, int chatId, string messageContent)
    {
        if(!(await HasPermissionToRead(userId, chatId))) 
        throw new InvalidOperationException($"User does not have permission to participate in chat with id:{chatId}");
        
        ReaderWriterLockSlim slimLock = locks.GetOrAdd(chatId, new ReaderWriterLockSlim());
        slimLock.EnterWriteLock();
      
        try
        {
            Message msg = new Message
            {
                AuthorId = userId,
                ChatId = chatId,
                Content = messageContent,
                CreatedAt = DateTime.Now
            };
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return _mapper.Map<MessageDto>(await GetMessageById(msg.Id));
        }
        finally
        {
             slimLock.ExitWriteLock();
        } 
    }

    public async Task<List<MessageDto>> GetAllMessages(int userId, int chatId)
    {   
        ReaderWriterLockSlim slimLock = locks.GetOrAdd(chatId, new ReaderWriterLockSlim());
        slimLock.EnterReadLock();
        try
        {
           return await _context.Messages
              .Include(x=> x.Author)
              .Include(x=> x.Chat)
              .Where(x => x.ChatId == chatId)
              .OrderBy(x => x.CreatedAt)
              .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
              .ToListAsync(); 
        }
        finally
        {
            slimLock.ExitReadLock();
        }
        
    }
    private async Task<Message> GetMessageById(int messageId)
    {
          return await _context.Messages
            .Include(x=> x.Author)
            .Include(x=> x.Chat)
            .Where(x => x.Id == messageId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasPermissionToRead(int userId, int chatId)
    {
      var chat = await _context.Chats.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == chatId);
      var isUserAdmin = chat.OwnerId == userId;
      var isUserMember = chat.Members.Exists(x => x.UserId == userId);
      return isUserAdmin || isUserMember;
    }

    public async Task MarkMessagesAsRead(int userId, int chatId)
    {
        List<int> unreadMessagesId = await _context.Messages
        .Include(x=> x.SeenBy)
        .Where(x => x.ChatId == chatId)
        .Where(x => x.AuthorId != userId)
        .Where(x=> !x.SeenBy.Any(y=> y.MessageId == x.Id && y.UserId == userId))
        .Select(x=> x.Id)
        .ToListAsync();

        if(unreadMessagesId.Any())
        {
          List<UserMessages> userMessages = new List<UserMessages>();
          foreach (int id in unreadMessagesId)
          {
              userMessages.Add(new UserMessages
              {
                MessageId = id,
                UserId = userId,
                SeenAt = DateTime.Now
              });
          }
          _context.UserMessages.AddRange(userMessages);
          await _context.SaveChangesAsync();
        }
    }
  }
}