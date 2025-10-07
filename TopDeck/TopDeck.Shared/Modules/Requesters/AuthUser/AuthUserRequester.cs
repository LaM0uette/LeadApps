using System.Security.Claims;
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
    
    public async Task<User?> GetAuthenticatedUserAsync()
    {
        AuthenticationState state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal userClaims = state.User;

        if (!(userClaims.Identity?.IsAuthenticated ?? false)) 
            return null;
        
        string? oAuthId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userName = userClaims.Identity.Name;

        if (oAuthId is null || userName is null) 
            return null;
        
        UserOAuthInputDTO dto = new("google-oauth2", oAuthId);
        User? user = await _userService.GetByOAuthAsync(dto);

        return user ?? null;
    }

    #endregion
}