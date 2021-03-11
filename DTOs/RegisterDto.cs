using System.ComponentModel.DataAnnotations;

namespace grup_gadu_api.DTOs
{
    public class RegisterDto
    {
        [Required] 
        public string Login { get; set; }
     
        [Required]
        public string Password { get; set; }
    }
}