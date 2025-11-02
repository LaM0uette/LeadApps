using System.Security.Claims;
using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Requesters.AuthUser;
using TopDeck.Domain.Models;
using TopDeck.Shared.UIStore;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Client.Layout;

public class MainLayoutBase : LayoutComponentBase
{
    #region Statements
    
    protected bool IsReady { get; set; }
    protected bool IsUserMenuOpen { get; set; }
    protected string CurrentUserName { get; set; } = string.Empty;
    protected string UserInitial => string.IsNullOrWhiteSpace(CurrentUserName) ? "?" : CurrentUserName.Trim()[0].ToString().ToUpperInvariant();
    
    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    [Inject] private IAuthUserRequester _authUserRequester { get; set; } = null!;
    [Inject] private UIStore _uiStore { get; set; } = null!;
    [Inject] private NavigationManager _nav { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;
    
    [CascadingParameter]
    private Task<AuthenticationState>? _authenticationStateTask { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        if (_authenticationStateTask is not null)
        {
            AuthenticationState state = await _authenticationStateTask;
            ClaimsPrincipal principal = state.User;

            if (principal.Identity?.IsAuthenticated ?? false)
            {
                User? user = await _authUserRequester.GetAuthenticatedUserAsync(principal);

                if (user is not null)
                {
                    _uiStore.Dispatch(new SetCurrentAuthenticatedUserAction(user.Id, user.Uuid));
                    CurrentUserName = user.UserName;

                    const string PlaceholderUserName = "__unknown__";
                    bool onPseudoPage = _nav.Uri.Contains("/profile/pseudo", StringComparison.OrdinalIgnoreCase);
                    if (string.Equals(CurrentUserName, PlaceholderUserName, StringComparison.Ordinal) && !onPseudoPage)
                    {
                        _nav.NavigateTo("/profile/pseudo", replace: true);
                        IsReady = true;
                        StateHasChanged();
                        return;
                    }
                    
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

    protected void ToggleUserMenu()
    {
        IsUserMenuOpen = !IsUserMenuOpen;
    }

    protected void CloseUserMenu()
    {
        if (IsUserMenuOpen)
        {
            IsUserMenuOpen = false;
            StateHasChanged();
        }
    }
    
    #endregion
}