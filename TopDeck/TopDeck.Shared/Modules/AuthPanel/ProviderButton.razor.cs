using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace AuthPanel;

public class ProviderButtonBase : LocalizedComponentBase
{
    #region Statements

    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "40px";
    [Parameter] public string IconPath { get; set; } = string.Empty;
    [Parameter] public string TextKey { get; set; } = string.Empty;
    [Parameter] public Provider Provider { get; set; }
    [Parameter] public string RedirectUrl { get; set; } = string.Empty;

    #endregion

    #region Methods
    
    // 

    #endregion
}