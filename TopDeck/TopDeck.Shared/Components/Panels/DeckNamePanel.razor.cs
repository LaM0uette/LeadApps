using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class DeckNamePanelBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Name { get; set; }
    [Parameter, EditorRequired] public required string Width { get; set; } = "100px";
    [Parameter, EditorRequired] public required string Height { get; set; } = "26px";
    [Parameter, EditorRequired] public required string FontSize { get; set; } = "0.8em";
    
    #endregion
}