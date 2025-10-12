using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IUserService
{
    Task<UserOutputDTO?> GetByOAuthIdAsync(AuthUserInputDTO dto, CancellationToken ct = default);
    Task<UserOutputDTO?> GetByUuidAsync(Guid uuid, CancellationToken ct = default);
    Task<UserOutputDTO> CreateAsync(UserInputDTO dto, CancellationToken ct = default);
    Task<UserOutputDTO?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}