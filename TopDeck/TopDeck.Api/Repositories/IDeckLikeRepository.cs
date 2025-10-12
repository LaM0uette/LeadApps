using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckLikeRepository
{
    Task<DeckLike?> GetByIdAsync(int deckId, int userId, CancellationToken ct = default);
    Task<DeckLike> AddAsync(DeckLike like, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default);
}