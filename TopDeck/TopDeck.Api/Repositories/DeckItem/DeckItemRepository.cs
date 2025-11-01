using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public class DeckItemRepository : IDeckItemRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public DeckItemRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion

    #region IRepository
    
    public DbSet<Deck> DbSet => _db.Decks;
    
    public async Task<IReadOnlyList<Deck>> GetAllAsync(bool includeAll, CancellationToken ct = default)
    {
        return await Query(includeAll).AsNoTracking().AsSplitQuery().OrderBy(dto => dto.CreatedAt).ThenBy(dto => dto.Id).ToListAsync(ct);
    }
    
    public async Task<Deck?> GetByIdAsync(int id, bool includeAll, CancellationToken ct = default)
    {
        return await Query(includeAll).AsNoTracking().FirstOrDefaultAsync(deck => deck.Id == id, ct);
    }
    
    public async Task<Deck> AddAsync(Deck deck, CancellationToken ct = default)
    {
        _db.Decks.Add(deck);
        await _db.SaveChangesAsync(ct);
        
        return deck;
    }

    public async Task<Deck> UpdateAsync(Deck deck, CancellationToken ct = default)
    {
        // Update only the scalar properties in the database using ExecuteUpdateAsync to avoid
        // tracker-related concurrency issues after bulk operations (ExecuteDelete/AddRange).
        int affected = await _db.Decks
            .Where(d => d.Id == deck.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Name, deck.Name)
                .SetProperty(d => d.EnergyIds, deck.EnergyIds.ToList())
                .SetProperty(d => d.UpdatedAt, deck.UpdatedAt)
            , ct);

        if (affected == 0)
        {
            // If no row was affected, the deck may have been deleted concurrently or the key is invalid
            throw new DbUpdateConcurrencyException($"Deck with id {deck.Id} was not updated because it no longer exists or was modified concurrently.");
        }

        return deck;
    }

    public async Task ReplaceDeckCardsAsync(int deckId, IEnumerable<DeckCard> newCards, CancellationToken ct = default)
    {
        // Hard replace: delete all existing DeckCards then insert provided ones
        await _db.DeckCards.Where(c => c.DeckId == deckId).ExecuteDeleteAsync(ct);
        if (newCards is not null)
        {
            // Ensure DeckId is set and navigation is null to avoid unexpected tracking
            var toAdd = newCards.Select(c => new DeckCard
            {
                DeckId = deckId,
                Deck = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber,
                IsHighlighted = c.IsHighlighted
            }).ToList();
            if (toAdd.Count > 0)
            {
                await _db.DeckCards.AddRangeAsync(toAdd, ct);
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceDeckTagsAsync(int deckId, IEnumerable<DeckTag> newTags, CancellationToken ct = default)
    {
        await _db.DeckTags.Where(t => t.DeckId == deckId).ExecuteDeleteAsync(ct);
        if (newTags is not null)
        {
            var toAdd = newTags
                .GroupBy(t => t.TagId) // ensure uniqueness
                .Select(g => new DeckTag
                {
                    DeckId = deckId,
                    Deck = null!,
                    TagId = g.Key,
                    Tag = null!
                }).ToList();
            if (toAdd.Count > 0)
            {
                await _db.DeckTags.AddRangeAsync(toAdd, ct);
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        Deck? existing = await _db.Decks.FirstOrDefaultAsync(deck => deck.Id == id, ct);
        
        if (existing is null) 
            return false;
        
        _db.Decks.Remove(existing);
        await _db.SaveChangesAsync(ct);
        
        return true;
    }
    
    
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _db.Decks.AnyAsync(d => d.Code == code, ct);
    }
    
    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _db.Decks.CountAsync(ct);
    }
    
    #endregion

    #region Methods
    
    private IQueryable<Deck> Query(bool includeAll)
    {
        return includeAll
            ? _db.Decks
                .Include(d => d.Creator)
                .Include(d => d.Cards)
                .Include(d => d.DeckTags)
                .ThenInclude(dt => dt.Tag)
                .Include(d => d.Suggestions)
                .ThenInclude(s => s.Suggestor)
                .Include(d => d.Suggestions)
                .ThenInclude(s => s.AddedCards)
                .Include(d => d.Suggestions)
                .ThenInclude(s => s.RemovedCards)
                .Include(d => d.Suggestions)
                .ThenInclude(s => s.Likes)
                .ThenInclude(l => l.User)
                .Include(d => d.Suggestions)
                .ThenInclude(s => s.Dislikes)
                .ThenInclude(dl => dl.User)
                .Include(d => d.Likes)
                .ThenInclude(l => l.User)
                .Include(d => d.Dislikes)
                .ThenInclude(dl => dl.User)
                .AsQueryable()
            : _db.Decks.AsQueryable();
    }

    #endregion
}