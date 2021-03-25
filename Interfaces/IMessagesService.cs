using System.Collections.Generic;
using System.Threading.Tasks;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Interfaces
{
    public interface IMessagesService
    {
         Task CreateMessage(int userId, int chatId, string messageContent);
         Task<List<MessageDto>> GetAllMessages(int userId, int chatId);
         Task<List<MessageDto>> GetUnreadMessages(int userId, int chatId);
    }
}