using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AuthPanel;

public class AuthPanelBase : LocalizedComponentBase
{
    #region Statements
    
    [Parameter] public string Width { get; set; } = "330px";
    [Parameter] public string Height { get; set; } = "500px";

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