using TopDeck.Domain.Models;

namespace Requesters.AuthUser;

public interface IAuthUserRequester
{
    Task<User?> GetAuthenticatedUserAsync();
}