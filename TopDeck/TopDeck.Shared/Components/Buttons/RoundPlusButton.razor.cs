using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class RoundPlusButtonPresenter : PresenterBase
{
    #region Statements
    
    [Parameter, EditorRequired] public required string Size { get; set; } = "26px";
    [Parameter] public EventCallback Clicked { get; set; }

    #endregion

    #region Methods

    //

    #endregion
}