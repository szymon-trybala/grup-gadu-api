using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using grup_gadu_api.Data;
using grup_gadu_api.Entities;
using grup_gadu_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Services
{
  public class MessagesService : IMessagesService
  {
    private static ConcurrentDictionary<int, ReaderWriterLockSlim> locks = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
    private readonly DataContext _context;
    public MessagesService(DataContext context)
    {
      _context = context;
    }
    public async Task CreateMessage(int userId, int chatId, string messageContent)
    {
        ReaderWriterLockSlim slimLock = locks.GetOrAdd(chatId, new ReaderWriterLockSlim());
        slimLock.EnterWriteLock();
        
        try
        {
            Thread.Sleep(10000 * chatId);
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

    public async Task<List<Message>> GetMessages(int chatId)
    {   
        ReaderWriterLockSlim slimLock = locks.GetOrAdd(chatId, new ReaderWriterLockSlim());
        slimLock.EnterReadLock();
        try
        {
            return await _context.Messages
              .Where(x => x.ChatId == chatId)
              .ToListAsync();
        }
        finally
        {
            slimLock.ExitReadLock();
        }
    }
  }
}