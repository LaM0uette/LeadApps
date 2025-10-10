using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckSuggestionLikeRepository : IDeckSuggestionLikeRepository
{
    private readonly ApplicationDbContext _db;

    public DeckSuggestionLikeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DeckSuggestionLike?> GetByIdAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckSuggestionLikes.Include(l => l.User).Include(l => l.Suggestion)
            .FirstOrDefaultAsync(l => l.DeckSuggestionId == deckSuggestionId && l.UserId == userId, ct);
    }

    public async Task<DeckSuggestionLike> AddAsync(DeckSuggestionLike like, CancellationToken ct = default)
    {
        _db.DeckSuggestionLikes.Add(like);
        await _db.SaveChangesAsync(ct);
        return like;
    }

    public async Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        DeckSuggestionLike? existing = await _db.DeckSuggestionLikes.FirstOrDefaultAsync(l => l.DeckSuggestionId == deckSuggestionId && l.UserId == userId, ct);
        if (existing is null) return false;
        _db.DeckSuggestionLikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}