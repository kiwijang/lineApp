using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<NotifyHist> NotifyHist { get; set; }
        public DbSet<NotifyData> NotifyData { get; set; }
    }

    public class DbContextFactory : IDbContextFactory<DataContext>
    {
        private static readonly AsyncLocal<DataContext> _dataContext = new AsyncLocal<DataContext>();

        private readonly IConfiguration _configuration;

        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataContext CreateDbContext()
        {
            if (_dataContext.Value == null)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                optionsBuilder.UseSqlite(connectionString);
                _dataContext.Value = new DataContext(optionsBuilder.Options);
            }

            return _dataContext.Value;
        }
    }

}
