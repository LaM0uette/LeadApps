using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface IDeckItemService
{
    Task<IReadOnlyList<Deck>> GetPageAsync(int skip, int take, CancellationToken ct = default);
    Task<Deck?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Deck> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default);
    Task<Deck?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
