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

    #region Repository
    
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
        _db.Decks.Update(deck);
        await _db.SaveChangesAsync(ct);
        
        return deck;
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