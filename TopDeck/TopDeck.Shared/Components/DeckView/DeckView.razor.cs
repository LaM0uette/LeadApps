using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;

namespace TopDeck.Shared.Components;

public class DeckViewBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required Deck Deck { get; set; }
    
    protected IReadOnlyList<TCGPCard> HighlightedCards { get; set; } = [];

    protected string DeckCode = string.Empty;
    
    protected  Dictionary<int, string> EnergyTypes = new()
    {
        { 1, "Grass" },
        { 2, "Fire" },
        { 3, "Water" },
        { 4, "Lightning" },
        { 5, "Psychic" },
        { 6, "Fighting" },
        { 7, "Darkness" },
        { 8, "Metal" },
        { 9, "Dragon" },
        { 10, "Colorless" }
    };
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    

    protected override void OnParametersSet()
    {
        DeckCode = Deck.Code;
    }
    
    protected override async Task OnInitializedAsync()
    {
        List<TCGPCardRequest> cardRequests = [];
        cardRequests.AddRange(Deck.Cards
            .Where(c => c.IsHighlighted)
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
        );

        TCGPCardsRequest deckRequest = new(cardRequests);
        HighlightedCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest);
        
        Console.WriteLine($"HighlightedCards: {HighlightedCards.Count}");
        
        StateHasChanged();
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

    #endregion
}