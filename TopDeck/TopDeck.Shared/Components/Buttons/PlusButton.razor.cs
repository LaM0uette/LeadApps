using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class PlusButtonPresenter : PresenterBase
{
    #region Statements
    
    [Parameter, EditorRequired] public required string Width { get; set; } = "86px";
    [Parameter, EditorRequired] public required string Height { get; set; } = "120px";
    [Parameter] public EventCallback Clicked { get; set; }

    #endregion

    #region Methods

    //

    #endregion
}