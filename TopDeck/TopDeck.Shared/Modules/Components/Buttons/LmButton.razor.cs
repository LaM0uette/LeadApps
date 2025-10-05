using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class LmButtonBase : LocalizedComponentBase
{
    #region Statements

    [Parameter] public string style { get; set; } = string.Empty;
    
    [Parameter] public string Width { get; set; } = "auto";
    [Parameter] public string Height { get; set; } = "36px";
    [Parameter] public EventCallback Clicked { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion
}