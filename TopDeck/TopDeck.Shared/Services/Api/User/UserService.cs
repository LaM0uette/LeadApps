using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class UserService : ApiService, IUserService
{
    #region Statements

    private const string _route = "/users";

    public UserService(HttpClient http) : base(http) { }

    #endregion

    #region ApiService
    
    public async Task<User?> GetByOAuthIdAsync(AuthUserInputDTO dto, CancellationToken ct = default)
    {
        UserOutputDTO? response = await PostJsonAsync<AuthUserInputDTO, UserOutputDTO>($"{_route}/oauth", dto, ct);
        return response?.ToDomain();
    }
    
    public async Task<User?> GetByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        UserOutputDTO? response = await GetJsonAsync<UserOutputDTO>($"{_route}/uuid/{uuid}", ct);
        return response?.ToDomain();
    }

    public async Task<string?> GetNameByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        string? name = await GetJsonAsync<string>($"{_route}/uuid/{uuid}/name", ct);
        return name;
    }
    
    public async Task<User> CreateAsync(UserInputDTO dto, CancellationToken ct = default)
    {
        UserOutputDTO? response = await PostJsonAsync<UserInputDTO, UserOutputDTO>($"{_route}", dto, ct);
        return response?.ToDomain() ?? throw new Exception("Failed to create user");
    }
    
    public async Task<User?> UpdateAsync(int id, UserInputDTO dto, CancellationToken ct = default)
    {
        UserOutputDTO? response = await PutJsonAsync<UserInputDTO, UserOutputDTO>($"{_route}/{id}", dto, ct);
        return response?.ToDomain();
    }
    
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await DeleteAsync($"{_route}/{id}", ct);
    }

    #endregion
}
