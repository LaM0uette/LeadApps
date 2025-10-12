using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckDislikeRepository
{
    Task<DeckDislike?> GetByIdAsync(int deckId, int userId, CancellationToken ct = default);
    Task<DeckDislike> AddAsync(DeckDislike dislike, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default);
}
