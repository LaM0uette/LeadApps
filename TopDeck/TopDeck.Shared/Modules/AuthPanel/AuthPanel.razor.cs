using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AuthPanel;

public class AuthPanelBase : ComponentBase
{
    #region Statements

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