using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPocketDex.Contracts.Request;
using TCGPocketDex.Domain.Models;
using TopDeck.Domain.Models;
using TopDeck.Shared.Modules.Requesters.TCGPCard;

namespace TopDeck.Shared.Components;

public class DeckViewBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required Deck Deck { get; set; }
    
    protected IReadOnlyCollection<Card> Cards { get; set; } = [];

    protected string DeckCode = string.Empty;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private TCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override void OnParametersSet()
    {
        DeckCode = Deck.Code;
    }
    
    protected override void OnInitialized()
    {
        LoadCards();
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
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", Deck.Code);
        DeckCode = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        DeckCode = Deck.Code;
        StateHasChanged();
    }
    
    
    private async void LoadCards()
    {
        List<CardRequest> cardRequests = [];
        cardRequests.AddRange(Deck.Cards.Select(deckCard => new CardRequest(deckCard.CollectionCode, deckCard.CollectionNumber)));

        DeckRequest deckRequest = new(cardRequests);
        
        Cards = await _tcgpCardRequester.GetByBatchAsync(deckRequest);
        //Console.WriteLine($"Loaded {Cards.Count} cards for deck {Deck.Name}");
        StateHasChanged();
    }

    #endregion
}