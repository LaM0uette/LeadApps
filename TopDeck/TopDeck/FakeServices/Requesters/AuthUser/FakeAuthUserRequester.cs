using System.Security.Claims;
using TopDeck.Domain.Models;

namespace Requesters.AuthUser;

public class FakeAuthUserRequester : IAuthUserRequester
{
    public Task<User?> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
    {
        return Task.FromResult<User?>(null);
    }
}
