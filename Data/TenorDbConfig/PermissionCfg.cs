using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenor.Models;

namespace Tenor.Data.TenorDbConfig
{
    public class PermissionCfg : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
