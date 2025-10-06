using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

public interface IDeckService
{
    Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default);
    Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default);
    Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
