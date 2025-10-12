using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckDetailsService
{
    Task<DeckDetailsOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
    
    Task<DeckDetailsSuggestionOutputDTO> CreateSuggestionAsync(DeckSuggestionInputDTO dto, CancellationToken ct = default);
    Task<DeckDetailsSuggestionOutputDTO?> UpdateSuggestionAsync(int id, DeckSuggestionInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default);
}
