using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DeckDetailsBase : PresenterBase
{
    #region Enums

    protected enum Mode
    {
        View,
        Edit
    }
    
    protected enum Tab
    {
        Cards,
        Overview,
        Suggestions
    }

    #endregion
    
    #region Statements

    [Parameter, EditorRequired] public required string DeckCode { get; set; }

    protected Deck? Deck;
    protected IReadOnlyList<TCGPCard> Cards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> HighlightedCards { get; set; } = [];
    
    protected readonly Dictionary<int, string> EnergyTypes = new()
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
    
    protected Mode CurrentMode { get; set; } = Mode.View;
    protected Tab CurrentTab { get; set; } = Tab.Cards;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        Deck? deck = await _deckItemService.GetByCodeAsync(DeckCode);
        
        if (deck == null)
        {
            //_navigationManager.NavigateTo("/", true);
            return;
        }
        
        Deck = deck;
        
        List<TCGPCardRequest> cardRequests = [];
        cardRequests.AddRange(Deck.Cards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
        );

        TCGPCardsRequest deckRequest = new(cardRequests);
        Cards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest, loadThumbnail:true);
        HighlightedCards = Cards.Where(c => Deck.Cards.Any(dc => dc.IsHighlighted && dc.CollectionCode == c.Collection.Code && dc.CollectionNumber == c.CollectionNumber)).ToList();
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
    
    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }

    #endregion
}