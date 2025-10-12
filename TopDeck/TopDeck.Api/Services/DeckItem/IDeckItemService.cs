using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckItemService
{
    Task<IReadOnlyList<DeckItemOutputDTO>> GetPageAsync(int skip, int take, CancellationToken ct = default);
    Task<DeckItemOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<DeckItemOutputDTO> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default);
    Task<DeckItemOutputDTO?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}