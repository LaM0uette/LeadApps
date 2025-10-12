using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface IDeckDetailsService
{
    Task<DeckDetails?> GetByCodeAsync(string code, CancellationToken ct = default);
}
