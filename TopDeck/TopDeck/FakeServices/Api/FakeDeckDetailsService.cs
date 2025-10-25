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

    public Task<DeckDetailsSuggestion?> CreateSuggestionAsync(DeckSuggestionInputDTO suggestion, CancellationToken ct = default)
    {
        return Task.FromResult<DeckDetailsSuggestion?>(null);
    }
    
    public Task<DeckDetailsSuggestion?> UpdateSuggestionAsync(int id, DeckSuggestionInputDTO suggestion, CancellationToken ct = default)
    {
        return Task.FromResult<DeckDetailsSuggestion?>(null);
    }
    
    public Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }
}
