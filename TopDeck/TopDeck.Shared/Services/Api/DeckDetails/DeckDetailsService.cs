using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class DeckDetailsService : ApiService, IDeckDetailsService
{
    #region Statements

    private const string _route = "/deckDetails";
    
    public DeckDetailsService(HttpClient http) : base(http) { }

    #endregion

    #region ApiService
    
    public async Task<DeckDetails?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        DeckDetailsOutputDTO? dto = await GetJsonAsync<DeckDetailsOutputDTO>($"{_route}/{code}", ct);
        return dto?.ToDomain();
    }
    
    public async Task<DeckDetailsSuggestion?> CreateSuggestionAsync(DeckSuggestionInputDTO suggestion, CancellationToken ct = default)
    {
        DeckDetailsSuggestionOutputDTO? dto = await PostJsonAsync<DeckSuggestionInputDTO, DeckDetailsSuggestionOutputDTO>($"{_route}/suggestions", suggestion, ct);
        return dto?.ToDomain();
    }
    
    public async Task<DeckDetailsSuggestion?> UpdateSuggestionAsync(int id, DeckSuggestionInputDTO suggestion, CancellationToken ct = default)
    {
        DeckDetailsSuggestionOutputDTO? dto = await PutJsonAsync<DeckSuggestionInputDTO, DeckDetailsSuggestionOutputDTO>($"{_route}/suggestions/{id}", suggestion, ct);
        return dto?.ToDomain();
    }
    
    public async Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default)
    {
        return await DeleteAsync($"{_route}/suggestions/{id}", ct);
    }

    #endregion
}
