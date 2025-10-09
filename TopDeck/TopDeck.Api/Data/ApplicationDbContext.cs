using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    #region DbSets

    public DbSet<User> Users => Set<User>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();
    public DbSet<DeckSuggestion> DeckSuggestions => Set<DeckSuggestion>();
    public DbSet<DeckSuggestionAddedCard> DeckSuggestionAddedCards => Set<DeckSuggestionAddedCard>();
    public DbSet<DeckSuggestionRemovedCard> DeckSuggestionRemovedCards => Set<DeckSuggestionRemovedCard>();
    public DbSet<DeckLike> DeckLikes => Set<DeckLike>();
    public DbSet<DeckSuggestionLike> DeckSuggestionLikes => Set<DeckSuggestionLike>();
    public DbSet<DeckDislike> DeckDislikes => Set<DeckDislike>();
    public DbSet<DeckSuggestionDislike> DeckSuggestionDislikes => Set<DeckSuggestionDislike>();

    #endregion

    #region DbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("data");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.OAuthProvider).IsRequired();
            entity.Property(u => u.OAuthId).IsRequired();
            entity.Property(u => u.UserName).IsRequired();

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

            // Cards relation
            entity.HasMany(d => d.Cards)
                .WithOne(c => c.Deck)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            // Npgsql maps List<int> to integer[] automatically for energies
            entity.Property(d => d.EnergyIds).HasColumnType("integer[]");

            // Likes handled via DeckLike join entity
            entity.HasMany(d => d.Likes)
                .WithOne(l => l.Deck)
                .HasForeignKey(l => l.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dislikes handled via DeckDislike join entity
            entity.HasMany(d => d.Dislikes)
                .WithOne(l => l.Deck)
                .HasForeignKey(l => l.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Energies as arrays
            entity.Property(s => s.AddedEnergyIds).HasColumnType("integer[]");
            entity.Property(s => s.RemovedEnergyIds).HasColumnType("integer[]");

            // Cards relations
            entity.HasMany(s => s.AddedCards)
                .WithOne(c => c.Suggestion)
                .HasForeignKey(c => c.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.RemovedCards)
                .WithOne(c => c.Suggestion)
                .HasForeignKey(c => c.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Likes handled via DeckSuggestionLike join entity
            entity.HasMany(s => s.Likes)
                .WithOne(l => l.Suggestion)
                .HasForeignKey(l => l.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);

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

        // DeckLike
        modelBuilder.Entity<DeckLike>(entity =>
        {
            entity.HasKey(l => new { l.DeckId, l.UserId });
            entity.HasOne(l => l.Deck)
                .WithMany(d => d.Likes)
                .HasForeignKey(l => l.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(l => l.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        // DeckCard
        modelBuilder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CollectionCode).IsRequired();
            entity.HasIndex(c => new { c.DeckId, c.CollectionCode, c.CollectionNumber }).IsUnique();
            entity.HasOne(c => c.Deck)
                .WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeckSuggestionAddedCard
        modelBuilder.Entity<DeckSuggestionAddedCard>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CollectionCode).IsRequired();
            entity.HasIndex(c => new { c.DeckSuggestionId, c.CollectionCode, c.CollectionNumber }).IsUnique();
            entity.HasOne(c => c.Suggestion)
                .WithMany(s => s.AddedCards)
                .HasForeignKey(c => c.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeckSuggestionRemovedCard
        modelBuilder.Entity<DeckSuggestionRemovedCard>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CollectionCode).IsRequired();
            entity.HasIndex(c => new { c.DeckSuggestionId, c.CollectionCode, c.CollectionNumber }).IsUnique();
            entity.HasOne(c => c.Suggestion)
                .WithMany(s => s.RemovedCards)
                .HasForeignKey(c => c.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeckSuggestionLike
        modelBuilder.Entity<DeckSuggestionLike>(entity =>
        {
            entity.HasKey(l => new { l.DeckSuggestionId, l.UserId });
            entity.HasOne(l => l.Suggestion)
                .WithMany(s => s.Likes)
                .HasForeignKey(l => l.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(l => l.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        // DeckDislike
        modelBuilder.Entity<DeckDislike>(entity =>
        {
            entity.HasKey(l => new { l.DeckId, l.UserId });
            entity.HasOne(l => l.Deck)
                .WithMany(d => d.Dislikes)
                .HasForeignKey(l => l.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(l => l.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        // DeckSuggestionDislike
        modelBuilder.Entity<DeckSuggestionDislike>(entity =>
        {
            entity.HasKey(l => new { l.DeckSuggestionId, l.UserId });
            entity.HasOne(l => l.Suggestion)
                .WithMany(s => s.Dislikes)
                .HasForeignKey(l => l.DeckSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(l => l.CreatedAt)
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