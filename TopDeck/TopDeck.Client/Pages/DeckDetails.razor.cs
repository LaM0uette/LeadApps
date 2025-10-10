using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace TopDeck.Client.Pages;

public class DeckDetailsBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string DeckCode { get; set; }

    #endregion

    #region Methods

    //

    #endregion
}