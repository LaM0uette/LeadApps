using System.Security.Claims;
using Helpers.Auth0;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace Requesters.AuthUser;

public class AuthUserRequester : IAuthUserRequester
{
    #region Statements

    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserService _userService;

    public AuthUserRequester(AuthenticationStateProvider authenticationStateProvider, IUserService userService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userService = userService;
    }

    #endregion

    #region Methods
    
    public async Task<User?> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
    {
        string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Auth0SubHelper.TryParse(sub, out string provider, out string id)) 
            return null;

        UserOAuthInputDTO dto = new(provider, id);
        return await _userService.GetByOAuthAsync(dto);
    }

    #endregion
}