using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface IDeckDetailsService
{
    Task<DeckDetails?> GetByCodeAsync(string code, CancellationToken ct = default);
    
    Task<DeckDetailsSuggestion?> CreateSuggestionAsync(DeckSuggestionInputDTO suggestion, CancellationToken ct = default);
    Task<DeckDetailsSuggestion?> UpdateSuggestionAsync(int id, DeckSuggestionInputDTO suggestion, CancellationToken ct = default);
    Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default);
}
