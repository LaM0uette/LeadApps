using TopDeck.Shared.Models.TCGP;

namespace TCGPCardRequester;

public interface ITCGPCardRequester
{
    Task<List<TCGPCard>> GetAllTCGPCardsAsync(string? cultureOverride = null, bool loadThumbnail = false, CancellationToken ct = default);
    Task<List<TCGPCard>> GetTCGPCardsByRequestAsync(TCGPCardsRequest deck, string? cultureOverride = null, bool loadThumbnail = false, CancellationToken ct = default);
}