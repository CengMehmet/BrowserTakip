using System.Data.Entity;

namespace WindowsFormsApp1
{
    public class BrowserContext : DbContext
    {
        public BrowserContext(string cs) : base(cs)
        {
        }

        public DbSet<User> UserSet { get; set; }
        public DbSet<BrowserLog> BrowserLogSet { get; set; }
    }
}