using TCGPocketDex.Contracts.Request;
using TCGPocketDex.Domain.Models;
using TCGPocketDex.SDK.Services;
using TopDeck.Shared.Services.TCGPCard;

namespace TopDeck.Shared.Modules.Requesters.TCGPCard;

public class TCGPCardRequester
{
    private readonly ITCGPCardService _tcgpCardService;
    private readonly Dictionary<int, Card> _cache = new();

    public TCGPCardRequester(ITCGPCardService tcgpCardService)
    {
        _tcgpCardService = tcgpCardService;
    }

    public async Task<IReadOnlyCollection<Card>> GetAllAsync(CancellationToken ct = default)
    {
        var cards = await _tcgpCardService.GetAllAsync(null, ct);
        foreach (var card in cards)
            _cache[card.Id] = card;
        return cards;
    }

    public async Task<Card?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(id, out var card))
            return card;

        card = await _tcgpCardService.GetByIdAsync(id, null, ct);
        if (card != null)
            _cache[id] = card;

        return card;
    }

    public async Task<List<Card>> GetByBatchAsync(DeckRequest deck, string? cultureOverride = null, CancellationToken ct = default)
    {
        List<Card> cards = await _tcgpCardService.GetByBatchAsync(deck, cultureOverride, ct);
        /*foreach (Card card in cards)
            _cache[card.Id] = card;*/
        return cards;
    } 
}