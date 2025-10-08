using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class DeckDislikeRepository : IDeckDislikeRepository
{
    private readonly ApplicationDbContext _db;

    public DeckDislikeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DeckDislike?> GetByIdAsync(int deckId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckDislikes.Include(l => l.User).Include(l => l.Deck)
            .FirstOrDefaultAsync(l => l.DeckId == deckId && l.UserId == userId, ct);
    }

    public async Task<DeckDislike> AddAsync(DeckDislike dislike, CancellationToken ct = default)
    {
        _db.DeckDislikes.Add(dislike);
        await _db.SaveChangesAsync(ct);
        return dislike;
    }

    public async Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default)
    {
        DeckDislike? existing = await _db.DeckDislikes.FirstOrDefaultAsync(l => l.DeckId == deckId && l.UserId == userId, ct);
        if (existing is null) return false;
        _db.DeckDislikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
