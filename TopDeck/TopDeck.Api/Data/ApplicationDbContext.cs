using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    #region DbSets

    public DbSet<User> Users => Set<User>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckSuggestion> DeckSuggestions => Set<DeckSuggestion>();

    #endregion

    #region DbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.OAuthProvider).IsRequired();
            entity.Property(u => u.OAuthId).IsRequired();
            entity.Property(u => u.UserName).IsRequired();
            entity.Property(u => u.Email).IsRequired();

            entity.HasIndex(u => new { u.OAuthProvider, u.OAuthId }).IsUnique();

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        // Deck
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired();
            entity.Property(d => d.Code).IsRequired();
            entity.HasIndex(d => d.Code).IsUnique();

            // Npgsql mappe List<int> vers integer[] automatiquement
            entity.Property(d => d.CardIds).HasColumnType("integer[]");
            entity.Property(d => d.EnergyIds).HasColumnType("integer[]");

            entity.Property(d => d.Likes).HasDefaultValue(0);

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(d => d.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            entity.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        modelBuilder.Entity<DeckSuggestion>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.AddedCardIds).HasColumnType("integer[]");
            entity.Property(s => s.RemovedCardIds).HasColumnType("integer[]");
            entity.Property(s => s.AddedEnergyIds).HasColumnType("integer[]");
            entity.Property(s => s.RemovedEnergyIds).HasColumnType("integer[]");

            entity.Property(s => s.Likes).HasDefaultValue(0);

            entity.HasOne(s => s.Suggestor)
                .WithMany()
                .HasForeignKey(s => s.SuggestorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Deck)
                .WithMany(d => d.Suggestions)
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            entity.Property(s => s.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        DateTime utcNow = DateTime.UtcNow;

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) 
                continue;
            
            PropertyEntry? createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
            PropertyEntry? updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");

            if (entry.State == EntityState.Added && createdAtProp is { IsTemporary: false })
            {
                createdAtProp!.CurrentValue ??= utcNow;
            }

            if (updatedAtProp is not null)
            {
                updatedAtProp.CurrentValue = utcNow;
            }
        }
    }

    #endregion
}