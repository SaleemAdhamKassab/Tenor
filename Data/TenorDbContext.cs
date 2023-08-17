using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Tenor.Models;

namespace Tenor.Data
{
    public class TenorDbContext : DbContext
    {
        public TenorDbContext(DbContextOptions<TenorDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
                entity.SetTableName("TenorMeta" + entity.GetTableName());
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<MainSet> MainSets { get; set; }
        public DbSet<Subset> Subsets { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<KPI> KPIs { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Operator> Operators { get; set; }

    }
}