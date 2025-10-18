using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;

namespace TopDeck.Shared.Components;

public class DeckItemAddButtonPresenter : PresenterBase
{
    #region Statements
    
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Methods

    //

    #endregion
}