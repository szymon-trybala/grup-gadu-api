using System.Collections.Generic;
using System.Threading.Tasks;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByLoginAsync(string login);
    }
}