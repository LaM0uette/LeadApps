using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface IDeckItemService
{
    Task<IReadOnlyList<DeckItem>> GetPageAsync(int skip, int take, CancellationToken ct = default);
    Task<DeckItem?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<DeckItem> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default);
    Task<DeckItem?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<int> GetDeckItemCountAsync(CancellationToken ct = default);
}
