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
    public async Task<MessageDto>CreateMessage(int userId, int chatId, string messageContent)
    {
        if(!(await HasPermission(userId, chatId))) 
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

    private async Task<bool> HasPermission(int userId, int chatId)
    {
          return await _context.Chats
           .Include(x=> x.Members)
           .Where(x => x.Id == chatId)
           .Where(x=> x.Members.Any(x=> x.UserId == userId) || x.OwnerId == userId)
           .AnyAsync();
    }
  }
}