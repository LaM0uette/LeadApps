using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AuthPanel;

public class AuthPanelBase : ComponentBase
{
    #region Statements
    
    [Parameter] public string Width { get; set; } = "330px";
    [Parameter] public string Height { get; set; } = "500px";

    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }
    
    [Inject] protected ILocalizer Localizer { get; set; } = null!;

    protected string Username = "";

    protected override async Task OnInitializedAsync()
    {
        if (authenticationState is not null)
        {
            AuthenticationState state = await authenticationState;
            Username = state.User.Identity?.Name ?? string.Empty;
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Localizer.InitializeAsync(); // détecte navigator.language
            StateHasChanged();          // re-render avec la bonne culture
        }
    }

    #endregion
}