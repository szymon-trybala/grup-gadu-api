using System;
using System.Collections.Generic;

namespace grup_gadu_api.Entities
{
    public class Chat
    {
          public Chat()
          {
              Members = new List<UserChats>();
          }
          public int Id { get; set; }
          public string Name { get; set; }
          public DateTime CreatedAt { get; set; }
          public bool IsActive { get; set; }
          public int OwnerId { get; set; }
          public virtual AppUser Owner { get; set; }
          public virtual List<UserChats> Members { get; set; }
    }
}