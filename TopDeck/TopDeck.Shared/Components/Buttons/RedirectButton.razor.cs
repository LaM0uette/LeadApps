using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class RedirectButtonBase : LocalizedComponentBase
{
    #region Statements

    [Parameter] public string style { get; set; } = string.Empty;
    
    [Parameter] public string Width { get; set; } = "auto";
    [Parameter] public string Height { get; set; } = "36px";
    [Parameter] public string? Href { get; set; }
    [Parameter] public string Target { get; set; } = "_self";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion
}