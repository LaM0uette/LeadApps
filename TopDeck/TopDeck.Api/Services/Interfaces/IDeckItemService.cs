using TopDeck.Api.DTO;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IDeckItemService
{
    Task<IReadOnlyList<DeckOutputDTO>> GetDeckCardPageAsync(int skip, int take, CancellationToken ct = default);
    Task<DeckOutputDTO?> GetDeckCardByCodeAsync(string code, CancellationToken ct = default);
    Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default);
    Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}