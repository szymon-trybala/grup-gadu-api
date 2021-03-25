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
    public async Task CreateMessage(int userId, int chatId, string messageContent)
    {
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
            var result =  await _context.Messages
              .Include(x=> x.Author)
              .Include(x=> x.SeenBy)
              .Include(x=> x.Chat)
              .Where(x => x.ChatId == chatId)
              .OrderBy(x => x.CreatedAt)
              .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
              .ToListAsync();

              await MarkMessagesAsRead(userId,chatId);
              return result;
        }
        finally
        {
            slimLock.ExitReadLock();
        }
        
    }

    public async Task<List<MessageDto>> GetUnreadMessages(int userId, int chatId)
    {   
        ReaderWriterLockSlim slimLock = locks.GetOrAdd(chatId, new ReaderWriterLockSlim());
        slimLock.EnterReadLock();
        try
        {
            var result = await _context.Messages
              .Include(x=> x.Author)
              .Include(x=> x.SeenBy)
              .Include(x=> x.Chat)
              .Where(x => x.ChatId == chatId)
              .Where(x=> !x.SeenBy.Any(y=> y.MessageId == x.Id && y.UserId == userId))
              .OrderBy(x => x.CreatedAt)
              .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
              .ToListAsync();

              await MarkMessagesAsRead(userId,chatId);
              return result;
        }
        finally
        {
            slimLock.ExitReadLock();
        }
    }

    private async Task MarkMessagesAsRead(int userId, int chatId)
    {
        var unreadMessagesId = await _context.Messages
        .Include(x=> x.SeenBy)
        .Where(x => x.ChatId == chatId)
        .Where(x => x.AuthorId != userId)
        .Where(x=> !x.SeenBy.Any(y=> y.MessageId == x.Id && y.UserId == userId))
        .Select(x=> x.Id)
        .ToListAsync();

        if(unreadMessagesId.Count == 0) return;

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