using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class DeckItemService : ApiService, IDeckItemService
{
    #region Statements

    private const string _route = "/deckItems";

    #endregion

    #region ApiService

    public async Task<IReadOnlyList<DeckItem>> GetPageAsync(DeckItemsFilterDTO filter, CancellationToken ct = default)
    {
        // Server expects POST body for paging/filtering
        var safe = new DeckItemsFilterDTO
        {
            Skip = filter.Skip < 0 ? 0 : filter.Skip,
            Take = filter.Take <= 0 ? 20 : filter.Take,
            Search = filter.Search,
            TagIds = filter.TagIds is { Count: > 0 } ? filter.TagIds.Distinct().ToList() : null,
            OrderBy = filter.OrderBy,
            Asc = filter.Asc
        };
        IReadOnlyList<DeckItemOutputDTO>? result = await PostJsonAsync<DeckItemsFilterDTO, IReadOnlyList<DeckItemOutputDTO>>($"{_route}/page", safe, ct);
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
    
    public async Task<int> GetDeckItemCountAsync(string? search = null, IReadOnlyList<int>? tagIds = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (tagIds is { Count: > 0 })
        {
            foreach (int id in tagIds.Distinct())
                query.Add($"tagIds={id}");
        }
        string url = query.Count == 0 ? $"{_route}/count" : $"{_route}/count?{string.Join("&", query)}";
        int? count = await GetJsonAsync<int>(url, ct);
        return count ?? 0;
    }

    #endregion
}
