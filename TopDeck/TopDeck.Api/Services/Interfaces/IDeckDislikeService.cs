using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IDeckDislikeService
{
    Task<DeckDislikeOutputDTO?> CreateAsync(DeckDislikeInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default);
}
