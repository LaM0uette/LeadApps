using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserOutputDTO>> GetAllAsync(CancellationToken ct = default);
    Task<UserOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserOutputDTO?> GetByOAuthAsync(UserOAuthInputDTO dto, CancellationToken ct = default);
    Task<UserOutputDTO> CreateAsync(UserInputDTO dto, CancellationToken ct = default);
    Task<UserOutputDTO?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}