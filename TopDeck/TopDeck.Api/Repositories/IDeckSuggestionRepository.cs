using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckSuggestionRepository
{
    Task<IReadOnlyList<DeckSuggestion>> GetAllAsync(bool includeRelations = false, CancellationToken ct = default);
    Task<DeckSuggestion?> GetByIdAsync(int id, bool includeRelations = true, CancellationToken ct = default);
    Task<DeckSuggestion> AddAsync(DeckSuggestion suggestion, CancellationToken ct = default);
    Task<DeckSuggestion> UpdateAsync(DeckSuggestion suggestion, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}