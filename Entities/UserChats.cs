namespace grup_gadu_api.Entities
{
    public class UserChats
    {
    public int UserId { get; set; }
    public AppUser User { get; set; }
    public int ChatId { get; set; }
    public Chat Chat { get; set; }
    }
}