using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TopDeck.Contracts.AuthenticationStateSyncer;

public class PersistentAuthenticationStateProvider(PersistentComponentState persistentState) : AuthenticationStateProvider
{
    #region Statements

    private static readonly Task<AuthenticationState> _unauthenticatedTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    #endregion

    #region AuthenticationStateProvider

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!persistentState.TryTakeFromJson(nameof(OAuthUserInfo), out OAuthUserInfo? userInfo) || userInfo is null)
            return _unauthenticatedTask;

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, userInfo.Id),
            new(ClaimTypes.Name, userInfo.Name),
            new(ClaimTypes.Email, userInfo.Email)
        ];

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: nameof(PersistentAuthenticationStateProvider)))));
    }

    #endregion
}