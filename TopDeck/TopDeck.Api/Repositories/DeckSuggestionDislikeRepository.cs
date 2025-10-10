using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckSuggestionDislikeRepository : IDeckSuggestionDislikeRepository
{
    private readonly ApplicationDbContext _db;

    public DeckSuggestionDislikeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DeckSuggestionDislike?> GetByIdAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckSuggestionDislikes.Include(l => l.User).Include(l => l.Suggestion)
            .FirstOrDefaultAsync(l => l.DeckSuggestionId == deckSuggestionId && l.UserId == userId, ct);
    }

    public async Task<DeckSuggestionDislike> AddAsync(DeckSuggestionDislike dislike, CancellationToken ct = default)
    {
        _db.DeckSuggestionDislikes.Add(dislike);
        await _db.SaveChangesAsync(ct);
        return dislike;
    }

    public async Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        DeckSuggestionDislike? existing = await _db.DeckSuggestionDislikes.FirstOrDefaultAsync(l => l.DeckSuggestionId == deckSuggestionId && l.UserId == userId, ct);
        if (existing is null) return false;
        _db.DeckSuggestionDislikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
