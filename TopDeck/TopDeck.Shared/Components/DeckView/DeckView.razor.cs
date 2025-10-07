using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class DeckViewBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required Deck Deck { get; set; }

    #endregion

    #region Methods

    //

    #endregion
}