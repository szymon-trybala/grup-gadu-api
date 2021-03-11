using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if(await context.Users.AnyAsync()) return;

            string userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            List<AppUser> users = JsonConvert.DeserializeObject<List<AppUser>>(userData);

            foreach(AppUser user in users)
            {
              using var hmac = new HMACSHA512();
              user.Login = user.Login.ToLower();
              user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
              user.PasswordSalt = hmac.Key;

              context.Users.Add(user); 
            }
            await context.SaveChangesAsync();
        }
    }
}