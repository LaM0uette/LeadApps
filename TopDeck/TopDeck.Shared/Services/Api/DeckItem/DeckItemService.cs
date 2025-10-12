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

    public async Task<IReadOnlyList<DeckItem>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        string url = $"{_route}/page?skip={skip}&take={take}";
        IReadOnlyList<DeckItemOutputDTO>? result = await GetJsonAsync<IReadOnlyList<DeckItemOutputDTO>>(url, ct);
        return result?.ToDomain() ?? [];
    }
    
    public async Task<DeckItem?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        DeckItemOutputDTO? dto = await GetJsonAsync<DeckItemOutputDTO>($"{_route}/deckItem/{code}", ct);
        return dto?.ToDomain();
    }

    public async Task<DeckItem> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default)
    {
        DeckItemOutputDTO? created = await PostJsonAsync<DeckItemInputDTO, DeckItemOutputDTO>(_route, dto, ct);
        return created?.ToDomain() ?? throw new InvalidOperationException("Unexpected null response when creating deck.");
    }

    public async Task<DeckItem?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default)
    {
        DeckItemOutputDTO? updated = await PutJsonAsync<DeckItemInputDTO, DeckItemOutputDTO>($"{_route}/{id}", dto, ct);
        return updated?.ToDomain();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return base.DeleteAsync($"{_route}/{id}", ct);
    }

    #endregion
}
