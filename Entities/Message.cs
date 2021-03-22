using System;

namespace grup_gadu_api.Entities
{
    public class Message
    {
          public int Id { get; set; }
          public int AuthorId { get; set; }
          public virtual AppUser Author { get; set; }
          public int ChatId { get; set; }
          public virtual Chat Chat { get; set; }
          public DateTime CreatedAt { get; set; }
          public string Content { get; set; }
}
}