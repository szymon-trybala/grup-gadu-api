using Microsoft.EntityFrameworkCore;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Data
{
  public class DataContext : DbContext
  {
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<UserChats> UserChats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserMessages> UserMessages { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserChats>()
            .HasKey(userChats => new { userChats.ChatId, userChats.UserId });  

        modelBuilder.Entity<UserMessages>()
            .HasKey(userMessages => new { userMessages.MessageId, userMessages.UserId });  
    }
  }
}