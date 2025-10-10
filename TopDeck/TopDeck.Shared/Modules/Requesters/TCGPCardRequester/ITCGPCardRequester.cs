using TopDeck.Shared.Models.TCGP;

namespace TCGPCardRequester;

public interface ITCGPCardRequester
{
    Task<List<TCGPCard>> GetTCGPCardsByRequestAsync(TCGPCardsRequest deck, string? cultureOverride = null, CancellationToken ct = default);
}