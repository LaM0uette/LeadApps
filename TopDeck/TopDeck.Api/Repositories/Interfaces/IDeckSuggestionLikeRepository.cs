using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories.Interfaces;

public interface IDeckSuggestionLikeRepository
{
    Task<DeckSuggestionLike?> GetByIdAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
    Task<DeckSuggestionLike> AddAsync(DeckSuggestionLike like, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
}