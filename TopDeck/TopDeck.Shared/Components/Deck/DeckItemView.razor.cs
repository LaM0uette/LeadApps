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
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override void OnParametersSet()
    {
        CodeText = DeckItem.Code;
    }
    
    protected override async Task OnInitializedAsync()
    {
        List<TCGPCardRequest> tcgpCardRequests = DeckItem.HighlightedCards
            .Select(c => new TCGPCardRequest(c.CollectionCode, c.CollectionNumber))
            .ToList();

        TCGPCardsRequest tcgpCardsRequest = new(tcgpCardRequests);
        HighlightedTCGPCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(tcgpCardsRequest, loadThumbnail:true);
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
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", DeckItem.Code);
        CodeText = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        CodeText = DeckItem.Code;
        StateHasChanged();
    }
    
    protected void OpenDeckDetails()
    {
        string url = $"/decks/{DeckItem.Code}";
        _navigationManager.NavigateTo(url);
    }

    #endregion
}