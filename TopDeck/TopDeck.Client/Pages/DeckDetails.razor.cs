using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DeckDetailsBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string DeckCode { get; set; }

    protected Deck? Deck;
    protected IReadOnlyList<TCGPCard> HighlightedCards { get; set; } = [];
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        Deck? deck = await _deckService.GetByCodeAsync(DeckCode);
        
        if (deck == null)
        {
            //_navigationManager.NavigateTo("/", true);
            return;
        }
        
        Deck = deck;
        
        List<TCGPCardRequest> cardRequests = [];
        cardRequests.AddRange(Deck.Cards
            .Where(c => c.IsHighlighted)
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
        );

        TCGPCardsRequest deckRequest = new(cardRequests);
        HighlightedCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest);
        Console.WriteLine(HighlightedCards.Count);
    }

    #endregion

    #region Methods

    protected string GetEnergyClass(IEnumerable<int> energieIds)
    {
        int id = energieIds.FirstOrDefault();
        return id <= 0 ? "energy-none" : $"energy-{id}";
    }
    
    protected async Task CopyCode()
    {
        if (Deck == null) 
            return;
        
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", Deck.Code);
        DeckCode = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        DeckCode = Deck.Code;
        StateHasChanged();
    }

    #endregion
}