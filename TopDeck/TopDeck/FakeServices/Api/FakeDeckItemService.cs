using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeDeckItemService : IDeckItemService
{
    private static readonly List<DeckItem> _all;

    static FakeDeckItemService()
    {
        _all = Enumerable.Range(1, 200)
            .Select(i => new DeckItem(
                Id: i,
                CreatorUui: $"user-{(i % 10) + 1}",
                Name: $"Deck {i}",
                Code: $"DECK-{i:D4}",
                HighlightedCards: new List<DeckItemCard>
                {
                    new DeckItemCard("SV", (i % 180) + 1),
                    new DeckItemCard("SV", ((i * 7) % 180) + 1)
                },
                EnergyIds: new List<int> { (i % 9) + 1, ((i + 2) % 9) + 1 },
                TagIds: new List<int> { (i % 5) + 1 },
                LikeUserUuids: new List<string>(),
                DislikeUserUuids: new List<string>(),
                CreatedAt: DateTime.UtcNow.AddDays(-i)
            ))
            .ToList();
    }

    public Task<IReadOnlyList<DeckItem>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) return Task.FromResult<IReadOnlyList<DeckItem>>(Array.Empty<DeckItem>());
        var page = _all.Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<DeckItem>>(page);
    }
    
    public Task<DeckItem?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var item = _all.FirstOrDefault(d => string.Equals(d.Code, code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(item);
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
