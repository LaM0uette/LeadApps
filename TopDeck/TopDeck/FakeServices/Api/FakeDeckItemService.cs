using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeDeckItemService : IDeckItemService
{
    public Task<IReadOnlyList<DeckItem>> GetPageAsync(DeckItemsFilterDTO filter, CancellationToken ct = default)
    {
        IReadOnlyList<DeckItem> result = [];
        return Task.FromResult(result);
    }

    public Task<int> GetDeckItemCountAsync(string? search = null, IReadOnlyList<int>? tagIds = null, CancellationToken ct = default)
    {
        return Task.FromResult(0);
    }
    
    public Task<DeckItem?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return Task.FromResult<DeckItem?>(null);
    }

    public Task<DeckItem> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeckItem?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}