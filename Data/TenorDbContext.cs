﻿using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
		public DbSet<UserToken> UserTokens { get; set; }
		public DbSet<ExtraField> ExtraFields { get; set; }
		public DbSet<KpiField> KpiFields { get; set; }
		public DbSet<CounterField> CounterFields { get; set; }
		public DbSet<ReportField> ReportFields { get; set; }
		public DbSet<ReportFilter> ReportFilters { get; set; }
		public DbSet<ReportMeasure> ReportMeasures { get; set; }
		public DbSet<MeasureHaving> MeasureHavings { get; set; }
		public DbSet<DashboardField> DashboardFields { get; set; }
		public DbSet<KpiFieldValue> KpiFieldValues { get; set; }
		public DbSet<CounterFieldValue> CounterFieldValues { get; set; }
		public DbSet<ReportFieldValue> ReportFieldValues { get; set; }
		public DbSet<DashboardFieldValue> DashboardFieldValues { get; set; }
		public DbSet<Report> Reports { get; set; }
		public DbSet<ReportLevel> ReportLevels { get; set; }
		public DbSet<Dashboard> Dashboards { get; set; }
		public DbSet<SubsetField> SubsetFields { get; set; }
		public DbSet<SubsetFieldValue> SubsetFieldValues { get; set; }
		public DbSet<Level> Levels { get; set; }
		public DbSet<Dimension> Dimensions { get; set; }
		public DbSet<DimensionJoiner> DimensionJoiners { get; set; }
		public DbSet<DimensionLevel> DimensionLevels { get; set; }
		public DbSet<ReportFilterContainer> ReportFilterContainers { get; set; }

        //Entity Config
        protected override void OnModelCreating(ModelBuilder builder)
		{
			//Add Entities Configuration
			builder.ApplyConfiguration(new RoleCfg());
			builder.ApplyConfiguration(new PermissionCfg());
			builder.ApplyConfiguration(new ExtraFieldCfg());
		}
	}

}



