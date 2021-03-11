using Microsoft.EntityFrameworkCore;
using grup_gadu_api.Entities;

namespace grup_gadu_api.Data
{
  public class DataContext : DbContext
  {
    public DbSet<AppUser> Users { get; set; }
    public DataContext(DbContextOptions options) : base(options)
    {
    }
  }
}