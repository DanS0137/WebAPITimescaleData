using System;

namespace WebAPITimescaleData.Data
{
    public class WebApiAppDbContext : DbContext
    {
        public WebApiAppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
