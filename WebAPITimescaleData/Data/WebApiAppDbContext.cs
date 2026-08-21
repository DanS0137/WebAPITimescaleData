using Microsoft.EntityFrameworkCore;
using WebAPITimescaleData.Model;

namespace WebAPITimescaleData.Data
{
    public class WebApiAppDbContext : DbContext
    {
        public WebApiAppDbContext(DbContextOptions<WebApiAppDbContext> options)
            : base(options) { }

        public DbSet<Record> Values { get; set; }
        public DbSet<Result> Results { get; set; }
    }
}
