using Microsoft.EntityFrameworkCore;
using Prober.Entities;

namespace Prober.Data.AppDb;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Application> Applications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new ApplicationEntityConfiguration().Configure(modelBuilder.Entity<Application>());
    }
}
