using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeDeckItemService : IDeckItemService
{
    public Task<IReadOnlyList<Deck>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Deck> result = [];
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Deck>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        IReadOnlyList<Deck> result = [];
        return Task.FromResult(result);
    }

    public Task<Deck?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult<Deck?>(null);
    }
    
    public Task<Deck?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return Task.FromResult<Deck?>(null);
    }

    public Task<Deck> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Deck?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
