using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using TCGPocketDex.Domain.Models;
using TopDeck.Domain.Models;
using TopDeck.Shared.Modules.Requesters.TCGPCard;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class HomeBase : LocalizedComponentBase
{
    #region Statements
    
    protected IReadOnlyList<Deck> Decks { get; set; } = [];

    [Inject] private IDeckService _deckService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        Decks = await _deckService.GetAllAsync();
        StateHasChanged();
    }

    #endregion

}