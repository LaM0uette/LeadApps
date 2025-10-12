using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class DeckDetailsService : ApiService, IDeckDetailsService
{
    #region Statements

    private const string _route = "/api/deckDetails";

    #endregion

    #region ApiService
    
    public async Task<DeckDetails?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        DeckDetailsOutputDTO? dto = await GetJsonAsync<DeckDetailsOutputDTO>($"{_route}/{code}", ct);
        return dto?.ToDomain();
    }

    #endregion
}
