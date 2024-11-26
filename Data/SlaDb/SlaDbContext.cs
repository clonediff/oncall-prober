using Microsoft.EntityFrameworkCore;
using Prober.Entities;

namespace Prober.Data.SlaDb;

public class SlaDbContext(DbContextOptions<SlaDbContext> options) : DbContext(options)
{
    public DbSet<Indicator> Indicators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new IndicatorEntityConfiguration().Configure(modelBuilder.Entity<Indicator>());
    }
}
