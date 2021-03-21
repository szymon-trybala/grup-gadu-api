using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<Chat> GetById(int chatId)
    {
       return await _context.Chats
                    .Where(x => x.IsActive)
                    .Include(x => x.Owner)
                    .Include(x => x.Members)
                    .FirstOrDefaultAsync(x => x.Id == chatId);
    }

    public async Task<Chat> GetByName(string chatName)
    {
      return await _context.Chats
                    .Where(x => x.IsActive)
                    .FirstOrDefaultAsync(x => x.Name == chatName);
    }

    public async Task<IEnumerable<Chat>> GetChatsAsync(int userId)
      {
          return await _context.Chats
                 .Include(x=> x.Owner)
                 .Include(x=> x.Members)
                 .ThenInclude(x => x.User)
                 .Where(chat => (chat.Members.Any(x=> x.UserId == userId) || chat.OwnerId == userId) && chat.IsActive)
                 .ToListAsync();
      }
  }
}