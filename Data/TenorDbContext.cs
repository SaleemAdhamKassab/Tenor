using Microsoft.EntityFrameworkCore;
using Tenor.Models;

namespace Tenor.Data
{
    public class TenorDbContext : DbContext
    {
        public TenorDbContext(DbContextOptions<TenorDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<MainSet> MainSets { get; set; }
        public DbSet<Subset> Subsets { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Operator> Operators { get; set; }

    }
}