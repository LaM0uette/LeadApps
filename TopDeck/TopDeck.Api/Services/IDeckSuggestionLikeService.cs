using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckSuggestionLikeService
{
    Task<DeckSuggestionLikeOutputDTO?> CreateAsync(DeckSuggestionLikeInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default);
}