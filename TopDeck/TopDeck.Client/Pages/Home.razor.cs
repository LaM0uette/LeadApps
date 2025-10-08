using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using TCGPocketDex.Domain.Models;
using TCGPocketDex.SDK.Services;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class HomeBase : LocalizedComponentBase
{
    #region Statements
    
    protected IReadOnlyList<Deck> Decks { get; set; } = [];
    protected IReadOnlyCollection<Card> Cards { get; set; } = [];

    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private ICardService _cardService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        Decks = await _deckService.GetAllAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) 
            return;

        Cards = await _cardService.GetAllAsync();
    }

    #endregion

    #region Methods

    //

    #endregion
}