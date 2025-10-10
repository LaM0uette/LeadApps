using System.Security.Claims;
using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Requesters.AuthUser;
using TopDeck.Domain.Models;
using TopDeck.Shared.UIStore;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Client.Layout;

public class MainLayoutBase : LayoutComponentBase
{
    #region Statements
    
    protected bool IsReady { get; set; }
    
    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    [Inject] private IAuthUserRequester _authUserRequester { get; set; } = null!;
    [Inject] private UIStore _uiStore { get; set; } = null!;
    
    [CascadingParameter]
    private Task<AuthenticationState>? _authenticationStateTask { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await Localizer.InitializeAsync();

        if (_authenticationStateTask is not null)
        {
            AuthenticationState state = await _authenticationStateTask;
            ClaimsPrincipal principal = state.User;

            if (principal.Identity?.IsAuthenticated ?? false)
            {
                User? user = await _authUserRequester.GetAuthenticatedUserAsync(principal);

                if (user is not null)
                {
                    _uiStore.Dispatch(new SetCurrentAuthenticatedUserAction(user.Id, user.OAuthId));
                    
                    IsReady = true;
                    StateHasChanged();
                }
            }
            else
            {
                IsReady = true;
                StateHasChanged();
            }
        }
    }
    
    #endregion
}