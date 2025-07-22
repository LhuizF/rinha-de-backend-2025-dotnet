using Microsoft.EntityFrameworkCore;
using Rinha.Domain.Entities;

namespace Rinha.Infra.Persistence
{
  public class RinhaDbContext : DbContext
  {
    public RinhaDbContext(DbContextOptions<RinhaDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Payment>().HasKey(p => p.CorrelationId);
      base.OnModelCreating(modelBuilder);
    }
  }
}
