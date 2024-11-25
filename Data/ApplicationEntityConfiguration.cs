using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prober.Entities;

namespace Prober.Data;

public class ApplicationEntityConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> entity)
    {
        entity.ToTable("application");
                
        entity.HasKey(a => a.Id);

        entity.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        entity.Property(a => a.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasColumnType("char(255)");

        entity.Property(a => a.Key)
            .HasColumnName("key")
            .IsRequired()
            .HasMaxLength(64);
    }
}
