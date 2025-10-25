using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TopDeck.Shared.Components;

public class TitlePanelBase : PresenterBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Name { get; set; } = string.Empty;
    [Parameter] public string Width { get; set; } = "100px";
    [Parameter] public string Height { get; set; } = "26px";
    [Parameter] public string FontSize { get; set; } = "0.63em";
    
    #endregion
}