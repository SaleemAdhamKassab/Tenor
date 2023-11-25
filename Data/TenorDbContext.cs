using Microsoft.EntityFrameworkCore;
using Tenor.Data.TenorDbConfig;
using Tenor.Models;

namespace Tenor.Data
{
    public class TenorDbContext : DbContext
    {
        public TenorDbContext(DbContextOptions<TenorDbContext> options) : base(options) { }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Subset> Subsets { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<GroupTenantRole> GroupTenantRoles { get; set; }
        public DbSet<UserTenantRole> UserTenantRoles { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }

        //Entity Config
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //Add Entities Configuration
            builder.ApplyConfiguration(new RoleCfg());
            builder.ApplyConfiguration(new PermissionCfg());

        }
    }
    
}
       
        
    
