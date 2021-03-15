using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using grup_gadu_api.Entities;
using System;

namespace grup_gadu_api.Data
{
    public class Seed
    {

        private static List<Chat> _chats {get;set;}
        private static List<AppUser> _users {get;set;}

        public static async Task SeedData(DataContext context)
        {
            await SeedUsers(context);
            await SeedChats(context);
            await SeedUserChats(context);
        }

        private static async Task SeedUsers(DataContext context)
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
            _users = users;
            await context.SaveChangesAsync();
        }

        private static async Task SeedChats(DataContext context)
        {
            if(await context.Chats.AnyAsync()) return;

            string chatData = await System.IO.File.ReadAllTextAsync("Data/ChatSeedData.json");
            List<Chat> chats = JsonConvert.DeserializeObject<List<Chat>>(chatData);

            foreach(Chat chat in chats)
            {
              chat.CreatedAt = DateTime.Now;
              chat.IsActive = true;

              context.Chats.Add(chat); 
            }
            _chats = chats;
               await context.SaveChangesAsync();
        }

        private static async Task SeedUserChats(DataContext context)
        {
            if(await context.UserChats.AnyAsync()) return;
            Random rnd = new Random();

            foreach(Chat chat in _chats)
            {
              int start = rnd.Next(0,_users.Count);
            
              foreach(AppUser user in _users.GetRange(start, _users.Count - start))
              {
                  context.UserChats.Add(new UserChats {Chat = chat, User = user});
              }
            }
            await context.SaveChangesAsync();
        }
    }
}