using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class TagBase : PresenterBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Name { get; set; } = string.Empty;
    
    #endregion
}