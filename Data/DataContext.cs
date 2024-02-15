using Microsoft.EntityFrameworkCore;

namespace Tenor.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}