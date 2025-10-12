using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class DeckItemService : ApiService, IDeckItemService
{
    #region Statements

    private const string _route = "/api/deckItems";

    #endregion

    #region ApiService

    public async Task<IReadOnlyList<Deck>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        string url = $"{_route}/page?skip={skip}&take={take}";
        IReadOnlyList<DeckOutputDTOold>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTOold>>(url, ct);
        List<Deck> list = result?.Select(d => d.ToDomain()).ToList() ?? [];
        return list;
    }
    
    public async Task<Deck?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        DeckOutputDTOold? dto = await GetJsonAsync<DeckOutputDTOold>($"{_route}/deckItem/{code}", ct);
        return dto?.ToDomain();
    }

    public async Task<Deck> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTOold? created = await PostJsonAsync<DeckInputDTO, DeckOutputDTOold>(_route, dto, ct);
        Deck? deck = created?.ToDomain();
        return deck ?? throw new InvalidOperationException("Unexpected null response when creating deck.");
    }

    public async Task<Deck?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTOold? updated = await PutJsonAsync<DeckInputDTO, DeckOutputDTOold>($"{_route}/{id}", dto, ct);
        return updated?.ToDomain();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return base.DeleteAsync($"{_route}/{id}", ct);
    }

    #endregion
}
