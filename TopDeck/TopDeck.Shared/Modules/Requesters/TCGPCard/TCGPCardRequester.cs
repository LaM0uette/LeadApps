using TCGPocketDex.Domain.Models;
using TCGPocketDex.SDK.Services;

namespace TopDeck.Shared.Modules.Requesters.TCGPCard;

public class TCGPCardRequester
{
    private readonly ICardService _cardService;
    private readonly Dictionary<int, Card> _cache = new();

    public TCGPCardRequester(ICardService cardService)
    {
        _cardService = cardService;
    }

    public async Task<IReadOnlyCollection<Card>> GetAllAsync(CancellationToken ct = default)
    {
        var cards = await _cardService.GetAllAsync(null, ct);
        foreach (var card in cards)
            _cache[card.Id] = card;
        return cards;
    }

    public async Task<Card?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(id, out var card))
            return card;

        card = await _cardService.GetByIdAsync(id, null, ct);
        if (card != null)
            _cache[id] = card;

        return card;
    }

    public async Task<IReadOnlyCollection<Card>> GetCardsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var results = new List<Card>();

        foreach (var id in ids)
        {
            var card = await GetByIdAsync(id, ct);
            if (card != null)
                results.Add(card);
        }

        return results;
    }
}