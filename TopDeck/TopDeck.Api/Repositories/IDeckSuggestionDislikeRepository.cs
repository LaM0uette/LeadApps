using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckSuggestionDislikeRepository
{
    Task<DeckSuggestionDislike?> GetByIdAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
    Task<DeckSuggestionDislike> AddAsync(DeckSuggestionDislike dislike, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
}
