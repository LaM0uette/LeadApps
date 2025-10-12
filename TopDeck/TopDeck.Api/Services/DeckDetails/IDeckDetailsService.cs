using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckDetailsService
{
    Task<DeckDetailsOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
}
