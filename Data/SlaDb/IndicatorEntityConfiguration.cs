using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prober.Entities;

namespace Prober.Data.SlaDb;

public class IndicatorEntityConfiguration : IEntityTypeConfiguration<Indicator>
{
    public void Configure(EntityTypeBuilder<Indicator> builder)
    {
        builder.HasKey(i => i.Id);

        builder
            .Property(i => i.IsBad)
            .HasDefaultValue(false);

        builder.HasIndex(i => i.Time);
        builder.HasIndex(i => i.Name);
    }
}
