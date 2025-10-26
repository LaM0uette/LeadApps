using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IDeckItemService
{
    Task<IReadOnlyList<DeckItemOutputDTO>> GetPageAsync(int skip, int take, string? search = null, IReadOnlyList<int>? tagIds = null, string? orderBy = null, bool asc = false, CancellationToken ct = default);
    Task<DeckItemOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<DeckItemOutputDTO> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default);
    Task<DeckItemOutputDTO?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(string? search = null, IReadOnlyList<int>? tagIds = null, CancellationToken ct = default);
}