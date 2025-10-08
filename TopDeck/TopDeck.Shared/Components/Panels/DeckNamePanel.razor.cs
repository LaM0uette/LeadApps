using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class DeckNamePanelBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Name { get; set; }
    
    #endregion
}