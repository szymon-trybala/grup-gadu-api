using System;

namespace grup_gadu_api.DTOs
{
    public class SeenByDto
    {
      public string Login { get; set; }
      public int Id { get; set; }
      public DateTime SeenAt { get; set; }
    }
}