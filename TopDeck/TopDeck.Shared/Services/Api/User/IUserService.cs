using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface IUserService
{
    Task<User?> GetByOAuthIdAsync(AuthUserInputDTO dto, CancellationToken ct = default);
    Task<User?> GetByUuidAsync(Guid uuid, CancellationToken ct = default);
    Task<string?> GetNameByUuidAsync(Guid uuid, CancellationToken ct = default);
    Task<User> CreateAsync(UserInputDTO dto, CancellationToken ct = default);
    Task<User?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
