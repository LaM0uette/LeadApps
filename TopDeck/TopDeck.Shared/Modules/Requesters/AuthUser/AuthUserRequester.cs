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
        ClaimsPrincipal userPrincipal = state.User;

        if (!(userPrincipal.Identity?.IsAuthenticated ?? false)) 
            return null;
        
        string? authId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? provider = authId?.Split('|').FirstOrDefault();

        if (authId is null || provider is null) 
            return null;
        
        UserOAuthInputDTO dto = new(provider, authId);
        User? user = await _userService.GetByOAuthAsync(dto);

        return user ?? null;
    }

    #endregion
}