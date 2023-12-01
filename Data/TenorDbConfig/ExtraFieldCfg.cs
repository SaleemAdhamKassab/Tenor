using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenor.Models;

namespace Tenor.Data.TenorDbConfig
{
    public class ExtraFieldCfg: IEntityTypeConfiguration<ExtraField>
    {
        public void Configure(EntityTypeBuilder<ExtraField> builder)
        {
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
