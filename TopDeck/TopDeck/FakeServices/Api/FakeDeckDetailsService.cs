using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeDeckDetailsService : IDeckDetailsService
{
    public Task<IReadOnlyList<DeckDetails>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        IReadOnlyList<DeckDetails> result = [];
        return Task.FromResult(result);
    }
    
    public Task<DeckDetails?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return Task.FromResult<DeckDetails?>(null);
    }

    /*public Task<DeckDetails> CreateAsync(DeckDetailsInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeckDetails?> UpdateAsync(int id, DeckDetailsInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }*/
}
