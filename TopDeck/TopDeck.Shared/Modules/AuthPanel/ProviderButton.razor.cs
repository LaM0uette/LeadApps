using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace AuthPanel;

public class ProviderButtonBase : AppComponentBase
{
    #region Statements

    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "40px";
    [Parameter] public string IconPath { get; set; } = string.Empty;
    [Parameter] public string TextKey { get; set; } = string.Empty;
    [Parameter] public Provider Provider { get; set; }
    
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    #endregion

    #region Methods
    
    protected void Login()
    {
        string providerName = GetProviderName();
        _navigationManager.NavigateTo($"/Account/Login?provider={providerName}", forceLoad: true);
    }
    
    
    private string GetProviderName()
    {
        return Provider switch
        {
            Provider.Google => "google-oauth2",
            Provider.Microsoft => "windowslive",
            Provider.Apple => "apple",
            Provider.Twitch => "twitch",
            Provider.Twitter => "twitter",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion
}