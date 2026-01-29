using LuminaryLife.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Data;

/// <summary>
/// Entity Framework database context for the LuminaryLife API
/// </summary>
public class CoreApiEfDbContext : DbContext
{
    public CoreApiEfDbContext(DbContextOptions<CoreApiEfDbContext> options) : base(options)
    {
    }

    public DbSet<AgencySite> AgencySites => Set<AgencySite>();
    public DbSet<AgencyTeam> AgencyTeams => Set<AgencyTeam>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AgencySite configuration
        modelBuilder.Entity<AgencySite>(entity =>
        {
            entity.ToTable("agency_sites");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UId).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.HasIndex(e => e.UId).IsUnique();
        });

        // AgencyTeam configuration
        modelBuilder.Entity<AgencyTeam>(entity =>
        {
            entity.ToTable("agency_teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UId).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ManagerUserId).HasMaxLength(36);
            entity.HasIndex(e => e.UId).IsUnique();
            
            entity.HasOne(e => e.AgencySite)
                .WithMany(s => s.AgencyTeams)
                .HasForeignKey(e => e.AgencySiteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            
            entity.HasOne(e => e.AgencySite)
                .WithMany(s => s.Users)
                .HasForeignKey(e => e.AgencySiteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AgencyTeam)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.AgencyTeamId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.HexColorCode).HasMaxLength(6);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // EntityTag configuration
        modelBuilder.Entity<EntityTag>(entity =>
        {
            entity.ToTable("entity_tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityId).HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.TagId }).IsUnique();
            
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.EntityTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
