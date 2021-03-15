using System.Collections.Generic;

namespace grup_gadu_api.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public virtual List<UserChats> Chats { get; set; }
    }
}