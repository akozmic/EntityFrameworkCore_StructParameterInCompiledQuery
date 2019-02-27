using Microsoft.EntityFrameworkCore;

namespace EFBug
{
    class BuggyDbContext : DbContext
    {
        public BuggyDbContext(DbContextOptions<BuggyDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        
    }
}