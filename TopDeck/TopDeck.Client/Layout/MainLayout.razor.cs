using System.Security.Claims;
using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Requesters.AuthUser;
using TopDeck.Domain.Models;

namespace TopDeck.Client.Layout;

public class MainLayoutBase : LayoutComponentBase
{
    #region Statements
    
    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    [Inject] private IAuthUserRequester _authUserRequester { get; set; } = null!;
    
    [CascadingParameter]
    private Task<AuthenticationState>? _authenticationStateTask { get; set; }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Localizer.InitializeAsync();
            
            if (_authenticationStateTask is not null)
            {
                AuthenticationState state = await _authenticationStateTask;
                ClaimsPrincipal principal = state.User;

                if (principal.Identity?.IsAuthenticated ?? false)
                {
                    User? user = await _authUserRequester.GetAuthenticatedUserAsync(principal);
                    Console.WriteLine(user?.OAuthId + " " + user?.UserName);
                }
            }
            
            StateHasChanged();
        }
    }
    
    #endregion
}