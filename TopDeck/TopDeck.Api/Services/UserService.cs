using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class UserService : IUserService
{
    #region Statements

    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    #endregion

    #region IService

    public async Task<UserOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        User? entity = await _repo.GetByIdAsync(id, ct);
        return entity?.ToOutput();
    }

    public async Task<IReadOnlyList<UserOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<User> users = await _repo.GetAllAsync(ct);
        return users.Select(u => u.ToOutput()).ToList();
    }

    public async Task<UserOutputDTO> CreateAsync(UserInputDTO dto, CancellationToken ct = default)
    {
        User? existing = await _repo.GetByOAuthAsync(dto.OAuthProvider, dto.OAuthId, ct);
        
        if (existing != null)
            return existing.ToOutput();

        User entity = dto.ToEntity();
        User created = await _repo.AddAsync(entity, ct);
        return created.ToOutput();
    }

    public async Task<UserOutputDTO?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default)
    {
        User? existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return null;
        existing.UpdateEntity(dto);
        User updated = await _repo.UpdateAsync(existing, ct);
        return updated.ToOutput();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _repo.DeleteAsync(id, ct);
    }

    #endregion
}