using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;

namespace TopDeck.Shared.Components;

public class DeckItemPresenter : PresenterBase
{
    #region Statements

    [Parameter, EditorRequired] public required DeckItem DeckItem { get; set; }
    
    protected string CodeText = string.Empty;
    protected IReadOnlyList<TCGPCard> HighlightedTCGPCards { get; private set; } = [];
    
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
    
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override void OnParametersSet()
    {
        CodeText = DeckItem.Code;
    }
    
    private string? _cardsLoadedForCode;
    
    protected override async Task OnParametersSetAsync()
    {
        // Skip fetching for placeholders or when no highlighted cards are available yet
        if (string.IsNullOrWhiteSpace(DeckItem.Code) || DeckItem.HighlightedCards == null || DeckItem.HighlightedCards.Count == 0)
        {
            HighlightedTCGPCards = Array.Empty<TCGPCard>();
            _cardsLoadedForCode = null;
            return;
        }
        
        // Avoid refetching if we already loaded cards for this deck code
        if (_cardsLoadedForCode == DeckItem.Code && HighlightedTCGPCards.Count > 0)
            return;
        
        List<TCGPCardRequest> tcgpCardRequests = DeckItem.HighlightedCards
            .Select(c => new TCGPCardRequest(c.CollectionCode, c.CollectionNumber))
            .ToList();

        TCGPCardsRequest tcgpCardsRequest = new(tcgpCardRequests);
        var cards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(tcgpCardsRequest, loadThumbnail:true);
        HighlightedTCGPCards = cards;
        _cardsLoadedForCode = DeckItem.Code;
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
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", DeckItem.Code);
        CodeText = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1400);
        CodeText = DeckItem.Code;
        StateHasChanged();
    }

    #endregion
}