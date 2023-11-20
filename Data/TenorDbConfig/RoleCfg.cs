using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenor.Models;

namespace Tenor.Data.TenorDbConfig
{
    public class RoleCfg : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
