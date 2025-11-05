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
    protected bool IsDarkTheme { get; set; }
    
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

    protected async Task ToggleUserMenu()
    {
        IsUserMenuOpen = !IsUserMenuOpen;
        if (IsUserMenuOpen)
        {
            try
            {
                var current = await _js.InvokeAsync<string>("TopDeckTheme.current");
                IsDarkTheme = string.Equals(current, "dark", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // ignore
            }
            StateHasChanged();
        }
    }

    protected async Task ToggleTheme()
    {
        try
        {
            await _js.InvokeVoidAsync("TopDeckTheme.toggle");
        }
        catch
        {
            // ignore
        }
    }

    protected async Task OnThemeSwitchChanged(ChangeEventArgs e)
    {
        try
        {
            bool isChecked = e.Value is bool b ? b : e.Value?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            IsDarkTheme = isChecked;
            await _js.InvokeVoidAsync("TopDeckTheme.set", isChecked ? "dark" : "light");
        }
        catch
        {
            // ignore
        }
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