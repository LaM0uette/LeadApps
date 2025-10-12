using System.Security.Claims;
using Helpers.Auth0;
using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace Requesters.AuthUser;

public class AuthUserRequester : IAuthUserRequester
{
    #region Statements

    private readonly IUserService _userService;

    public AuthUserRequester(IUserService userService)
    {
        _userService = userService;
    }

    #endregion

    #region Methods
    
    public async Task<User?> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
    {
        string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Auth0SubHelper.TryParse(sub, out string provider, out string oAuthId)) 
            return null;

        AuthUserInputDTO dto = new(provider, oAuthId);
        return await _userService.GetByOAuthIdAsync(dto);
    }

    #endregion
}