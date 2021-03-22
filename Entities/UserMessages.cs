using System;

namespace grup_gadu_api.Entities
{
    public class UserMessages
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int MessageId { get; set; }
        public Chat Message { get; set; }
        public DateTime SeenAt { get; set; }
    }
}