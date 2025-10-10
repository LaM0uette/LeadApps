using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class AutoRedirectBase : ComponentBase
{
    #region Statements
    
    [Parameter] public string? RedirectUri { get; set; }
    
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        if (!string.IsNullOrEmpty(RedirectUri))
        {
            _navigationManager.NavigateTo(RedirectUri);
        }
    }

    #endregion
}