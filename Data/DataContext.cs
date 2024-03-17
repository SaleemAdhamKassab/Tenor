using Microsoft.EntityFrameworkCore;
using Tenor.Data.TenorDbConfig;
using Tenor.Models;

namespace Tenor.Data
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }

		public DbSet<KPIResult> KPIResult { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<KPIResult>().HasNoKey();
		}
	}
}