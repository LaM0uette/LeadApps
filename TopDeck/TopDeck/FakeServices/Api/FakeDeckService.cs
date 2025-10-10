using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeDeckService : IDeckService
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

    public Task<Deck> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        throw new NotSupportedException("Deck creation is not supported in Blazor Server fake service.");
    }

    public Task<Deck?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        throw new NotSupportedException("Deck update is not supported in Blazor Server fake service.");
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotSupportedException("Deck deletion is not supported in Blazor Server fake service.");
    }
}
