using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeUserService : IUserService
{
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<User> result = Array.Empty<User>();
        return Task.FromResult(result);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult<User?>(null);
    }

    public Task<User?> GetByOAuthAsync(UserOAuthInputDTO dto, CancellationToken ct = default)
    {
        return Task.FromResult<User?>(null);
    }
}
