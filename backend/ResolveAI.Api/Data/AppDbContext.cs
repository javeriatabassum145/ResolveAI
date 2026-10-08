using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Models;

namespace ResolveAI.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();

    public DbSet<Investigation> Investigations => Set<Investigation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Refund>()
            .Property(r => r.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Investigation>()
            .Property(i => i.Confidence)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Events)
            .WithOne(e => e.Payment)
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Investigations)
            .WithOne(i => i.Payment)
            .HasForeignKey(i => i.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}