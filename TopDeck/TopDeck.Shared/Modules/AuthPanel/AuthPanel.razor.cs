using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TopDeck.Shared.Components;

namespace AuthPanel;

public class AuthPanelBase : PresenterBase
{
    #region Statements
    
    [Parameter] public string Width { get; set; } = "340px";
    [Parameter] public string Height { get; set; } = "620px";

    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }

    protected string Username = "";

    protected override async Task OnInitializedAsync()
    {
        if (authenticationState is not null)
        {
            AuthenticationState state = await authenticationState;
            Username = state.User.Identity?.Name ?? string.Empty;
        }
    }

    #endregion
}