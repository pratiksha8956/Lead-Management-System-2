using LeadManagement.Application.Abstractions;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<SalesRep> SalesReps => Set<SalesRep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesRep>(e =>
        {
            e.HasKey(x => x.SalesRepId);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasConversion<int>();
            e.HasOne(x => x.SalesRep)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.SalesRepId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Lead>(e =>
        {
            e.HasKey(x => x.LeadId);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.Source);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(50).IsRequired();
            e.Property(x => x.Company).HasMaxLength(200).IsRequired();
            e.Property(x => x.Position).HasMaxLength(200).IsRequired();
            e.Property(x => x.Source).HasMaxLength(100).IsRequired();
            e.Property(x => x.Priority).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<int>();
            e.HasOne(x => x.AssignedSalesRep)
                .WithMany(x => x.Leads)
                .HasForeignKey(x => x.AssignedSalesRepId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Interactions)
                .WithOne(x => x.Lead)
                .HasForeignKey(x => x.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interaction>(e =>
        {
            e.HasKey(x => x.InteractionId);
            e.Property(x => x.Notes).HasMaxLength(4000);
        });

        base.OnModelCreating(modelBuilder);
    }
}
