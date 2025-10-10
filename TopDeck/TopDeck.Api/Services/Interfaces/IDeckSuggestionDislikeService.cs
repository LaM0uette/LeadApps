using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IDeckSuggestionDislikeService
{
    Task<DeckSuggestionDislikeOutputDTO?> CreateAsync(DeckSuggestionDislikeInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
}
