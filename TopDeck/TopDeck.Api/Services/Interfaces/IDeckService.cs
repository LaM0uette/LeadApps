using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IDeckService
{
    Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DeckOutputDTO>> GetPageAsync(int skip, int take, CancellationToken ct = default);
    Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeckOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default);
    Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}