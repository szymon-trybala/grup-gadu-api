using System;
using System.Collections.Generic;

namespace grup_gadu_api.DTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerLogin { get; set; }
        public List<string> Members { get; set; }
    }
}