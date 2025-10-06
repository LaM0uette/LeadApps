using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckSuggestionRepository : IDeckSuggestionRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public DeckSuggestionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Repository
    
    public async Task<IReadOnlyList<DeckSuggestion>> GetAllAsync(bool includeRelations = false, CancellationToken ct = default)
    {
        return await Query(includeRelations).AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);
    }

    public async Task<DeckSuggestion?> GetByIdAsync(int id, bool includeRelations = true, CancellationToken ct = default)
    {
        return await Query(includeRelations).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<DeckSuggestion> AddAsync(DeckSuggestion suggestion, CancellationToken ct = default)
    {
        _db.DeckSuggestions.Add(suggestion);
        await _db.SaveChangesAsync(ct);
        return suggestion;
    }

    public async Task<DeckSuggestion> UpdateAsync(DeckSuggestion suggestion, CancellationToken ct = default)
    {
        _db.DeckSuggestions.Update(suggestion);
        await _db.SaveChangesAsync(ct);
        return suggestion;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        DeckSuggestion? existing = await _db.DeckSuggestions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (existing is null) return false;
        _db.DeckSuggestions.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    #endregion

    #region Methods

    private IQueryable<DeckSuggestion> Query(bool include)
    {
        return include
            ? _db.DeckSuggestions
                .Include(s => s.Suggestor)
                .Include(s => s.Deck)
                    .ThenInclude(d => d.Creator)
                .Include(s => s.Likes)
                    .ThenInclude(l => l.User)
            : _db.DeckSuggestions.AsQueryable();
    }

    #endregion
}