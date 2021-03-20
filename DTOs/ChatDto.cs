using System;
using System.Collections.Generic;

namespace grup_gadu_api.DTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public MemberDto Owner { get; set; }
        public List<MemberDto> Members { get; set; }
    }
}