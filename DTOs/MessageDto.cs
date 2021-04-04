using System;
using System.Collections.Generic;

namespace grup_gadu_api.DTOs
{
    public class MessageDto
    {
      public int Id { get; set; }
      public int AuthorId { get; set; }
      public string AuthorLogin { get; set; }
      public int ChatId { get; set; }
      public string ChatName { get; set; }
      public DateTime CreatedAt { get; set; }
      public string Content { get; set; }
    }
}