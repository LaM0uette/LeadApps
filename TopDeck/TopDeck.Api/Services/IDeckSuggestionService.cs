using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckSuggestionService
{
    Task<IReadOnlyList<DeckSuggestionOutputDTO>> GetAllAsync(CancellationToken ct = default);
    Task<DeckSuggestionOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeckSuggestionOutputDTO> CreateAsync(DeckSuggestionInputDTO dto, CancellationToken ct = default);
    Task<DeckSuggestionOutputDTO?> UpdateAsync(int id, DeckSuggestionInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}