using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckRepository : IDeckRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public DeckRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Repository
    
    public async Task<IReadOnlyList<Deck>> GetAllAsync(bool includeRelations = false, CancellationToken ct = default)
    {
        return await Query(includeRelations).AsNoTracking().OrderBy(d => d.Id).ToListAsync(ct);
    }

    public async Task<Deck?> GetByIdAsync(int id, bool includeRelations = true, CancellationToken ct = default)
    {
        return await Query(includeRelations).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
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
        Deck? existing = await _db.Decks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (existing is null) return false;
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

    private IQueryable<Deck> Query(bool include)
    {
        return include
            ? _db.Decks
                .Include(d => d.Creator)
                .Include(d => d.Suggestions)
                    .ThenInclude(s => s.Likes)
                        .ThenInclude(l => l.User)
                .Include(d => d.Likes)
                    .ThenInclude(l => l.User)
                .AsQueryable()
            : _db.Decks.AsQueryable();
    }

    #endregion
}