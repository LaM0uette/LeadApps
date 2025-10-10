using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckLikeRepository : IDeckLikeRepository
{
    private readonly ApplicationDbContext _db;

    public DeckLikeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DeckLike?> GetByIdAsync(int deckId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckLikes.Include(l => l.User).Include(l => l.Deck)
            .FirstOrDefaultAsync(l => l.DeckId == deckId && l.UserId == userId, ct);
    }

    public async Task<DeckLike> AddAsync(DeckLike like, CancellationToken ct = default)
    {
        _db.DeckLikes.Add(like);
        await _db.SaveChangesAsync(ct);
        return like;
    }

    public async Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default)
    {
        DeckLike? existing = await _db.DeckLikes.FirstOrDefaultAsync(l => l.DeckId == deckId && l.UserId == userId, ct);
        if (existing is null) return false;
        _db.DeckLikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}