using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

public class DeckService : ApiService, IDeckService
{
    private const string BasePath = "/api/decks";

    public async Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<DeckOutputDTO>? result = await GetJsonAsync<IReadOnlyList<DeckOutputDTO>>(BasePath, ct);
        return result ?? Array.Empty<DeckOutputDTO>();
    }

    public Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        => GetJsonAsync<DeckOutputDTO>($"{BasePath}/{id}", ct);

    public async Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        DeckOutputDTO? created = await PostJsonAsync<DeckInputDTO, DeckOutputDTO>(BasePath, dto, ct);
        return created ?? throw new InvalidOperationException("Unexpected null response when creating deck.");
    }

    public Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
        => PutJsonAsync<DeckInputDTO, DeckOutputDTO>($"{BasePath}/{id}", dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => base.DeleteAsync($"{BasePath}/{id}", ct);
}
