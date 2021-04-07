using System.Collections.Generic;
using System.Threading.Tasks;
using grup_gadu_api.DTOs;

namespace grup_gadu_api.Interfaces
{
    public interface IMessagesService
    {
         Task<MessageDto> CreateMessage(int userId, int chatId, string messageContent);
         Task<List<MessageDto>> GetAllMessages(int userId, int chatId);
         Task<int> CountUnreadMessages(int userId, int chatId);
         Task MarkMessagesAsRead(int userId, int chatId);
    }
}