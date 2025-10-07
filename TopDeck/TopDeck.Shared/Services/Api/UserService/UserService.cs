using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class UserService : ApiService, IUserService
{
    #region Statements

    private const string _route = "/api/users";

    #endregion

    #region ApiService

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<UserOutputDTO>? result = await GetJsonAsync<IReadOnlyList<UserOutputDTO>>(_route, ct);
        List<User> list = result?.Select(u => u.ToDomain()).ToList() ?? [];
        return list;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        UserOutputDTO? dto = await GetJsonAsync<UserOutputDTO>($"{_route}/{id}", ct);
        return dto?.ToDomain();
    }

    #endregion
}
