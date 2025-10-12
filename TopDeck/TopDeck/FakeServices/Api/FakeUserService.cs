using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

public class FakeUserService : IUserService
{
    public Task<User?> GetByOAuthIdAsync(AuthUserInputDTO dto, CancellationToken ct = default)
    {
        return Task.FromResult<User?>(null);
    }

    public Task<User?> GetByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        return Task.FromResult<User?>(null);
    }

    public Task<User> CreateAsync(UserInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<User?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
