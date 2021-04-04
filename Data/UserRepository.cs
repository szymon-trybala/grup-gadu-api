using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using grup_gadu_api.Entities;
using grup_gadu_api.Interfaces;

namespace grup_gadu_api.Data
{
   public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByLoginAsync(string login)
        {
            return await _context.Users
                .SingleOrDefaultAsync(x => x.Login == login);
        }
    }
}