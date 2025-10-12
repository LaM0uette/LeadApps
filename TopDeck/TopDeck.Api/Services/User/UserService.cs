using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
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
    
    public async Task<UserOutputDTO?> GetByOAuthIdAsync(AuthUserInputDTO dto, CancellationToken ct = default)
    {
        User? entity = await _repo.GetByOAuthIdAsync(dto.Provider, dto.OAuthId, ct);
        return entity?.MapToDTO();
    }
    
    public async Task<UserOutputDTO?> GetByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        User? entity = await _repo.GetByUuidAsync(uuid, ct);
        return entity?.MapToDTO();
    }

    public async Task<UserOutputDTO> CreateAsync(UserInputDTO dto, CancellationToken ct = default)
    {
        User? existing = await _repo.GetByOAuthIdAsync(dto.OAuthProvider, dto.OAuthId, ct);
        
        if (existing != null)
            return existing.MapToDTO();

        User entity = dto.ToEntity();
        User created = await _repo.AddAsync(entity, ct);
        return created.MapToDTO();
    }

    public async Task<UserOutputDTO?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default)
    {
        User? existing = await _repo.GetByIdAsync(id, ct);
        
        if (existing is null) 
            return null;
        
        existing.UpdateEntity(dto);
        User updated = await _repo.UpdateAsync(existing, ct);
        return updated.MapToDTO();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _repo.DeleteAsync(id, ct);
    }

    #endregion
}