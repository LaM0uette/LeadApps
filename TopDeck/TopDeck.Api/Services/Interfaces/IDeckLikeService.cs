using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IDeckLikeService
{
    Task<DeckLikeOutputDTO?> CreateAsync(DeckLikeInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default);
}