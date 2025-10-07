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
        IReadOnlyList<DeckOutputDTO>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTO>>(_route, ct);
        var list = result?.Select(d => d.ToDomain()).ToList() ?? new List<Deck>();
        return list;
    }

    public async Task<Deck?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        DeckOutputDTO? dto = await GetJsonAsync<DeckOutputDTO>($"{_route}/{id}", ct);
        return dto?.ToDomain();
    }

    public async Task<Deck> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTO? created = await PostJsonAsync<DeckInputDTO, DeckOutputDTO>(_route, dto, ct);
        var deck = created?.ToDomain();
        return deck ?? throw new InvalidOperationException("Unexpected null response when creating deck.");
    }

    public async Task<Deck?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTO? updated = await PutJsonAsync<DeckInputDTO, DeckOutputDTO>($"{_route}/{id}", dto, ct);
        return updated?.ToDomain();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return base.DeleteAsync($"{_route}/{id}", ct);
    }

    #endregion
}
