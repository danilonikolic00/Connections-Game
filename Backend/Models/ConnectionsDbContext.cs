using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class ConnectionsDBContext : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Term> Terms { get; set; }
        public ConnectionsDBContext(DbContextOptions options) : base(options)
        {

        }
    }
}