using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class DeckService : ApiService, IDeckService
{
    #region Statements

    private const string _route = "/api/decks";

    #endregion

    #region ApiService

    public async Task<IReadOnlyList<Deck>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<DeckOutputDTOold>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTOold>>(_route, ct);
        var list = result?.Select(d => d.ToDomain()).ToList() ?? new List<Deck>();
        return list;
    }

    public async Task<IReadOnlyList<Deck>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        string url = $"{_route}/page?skip={skip}&take={take}";
        IReadOnlyList<DeckOutputDTOold>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTOold>>(url, ct);
        List<Deck> list = result?.Select(d => d.ToDomain()).ToList() ?? [];
        return list;
    }

    public async Task<Deck?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        DeckOutputDTOold? dto = await GetJsonAsync<DeckOutputDTOold>($"{_route}/{id}", ct);
        return dto?.ToDomain();
    }
    
    public async Task<Deck?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        DeckOutputDTOold? dto = await GetJsonAsync<DeckOutputDTOold>($"{_route}/deck/{code}", ct);
        return dto?.ToDomain();
    }

    public async Task<Deck> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTOold? created = await PostJsonAsync<DeckInputDTO, DeckOutputDTOold>(_route, dto, ct);
        var deck = created?.ToDomain();
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
