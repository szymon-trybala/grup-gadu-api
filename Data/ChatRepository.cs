using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using grup_gadu_api.Entities;
using grup_gadu_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace grup_gadu_api.Data
{
  public class ChatRepository : IChatRepository
  {
      private readonly DataContext _context;
      public ChatRepository(DataContext context)
      {
        _context = context;
      }

      public async Task<IEnumerable<Chat>> GetChatsAsync(int userId)
      {
          return await _context.Chats
                 .Include(x=> x.Owner)
                 .Include(x=> x.Members)
                 .Where(chat => chat.Members.Any(x=> x.UserId == userId) || chat.OwnerId == userId)
                 .ToListAsync();
      }
  }
}