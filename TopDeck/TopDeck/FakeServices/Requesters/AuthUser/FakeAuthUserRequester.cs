using TopDeck.Domain.Models;

namespace Requesters.AuthUser;

public class FakeAuthUserRequester : IAuthUserRequester
{
    public Task<User?> GetAuthenticatedUserAsync()
    {
        return Task.FromResult<User?>(null);
    }
}
