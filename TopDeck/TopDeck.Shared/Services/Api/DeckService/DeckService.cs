using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

public class DeckService : ApiService, IDeckService
{
    #region Statements

    private const string _route = "/decks";

    #endregion

    #region ApiService

    public async Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<DeckOutputDTO>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTO>>(_route, ct);
        return result ?? [];
    }

    public Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return GetJsonAsync<DeckOutputDTO>($"{_route}/{id}", ct);
    }

    public async Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTO? created = await PostJsonAsync<DeckInputDTO, DeckOutputDTO>(_route, dto, ct);
        return created ?? throw new InvalidOperationException("Unexpected null response when creating deck.");
    }

    public Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        return PutJsonAsync<DeckInputDTO, DeckOutputDTO>($"{_route}/{id}", dto, ct);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return base.DeleteAsync($"{_route}/{id}", ct);
    }

    #endregion
}
