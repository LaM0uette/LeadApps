using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;

namespace TopDeck.Shared.Components;

public class DeckItemPresenter : PresenterBase
{
    #region Statements

    [Parameter, EditorRequired] public required DeckItem DeckItem { get; set; }
    
    protected string CodeText = string.Empty;
    protected IReadOnlyList<TCGPCard> HighlightedTCGPCards { get; private set; } = [];
    protected string? CreatorName { get; private set; }
    
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
    
    // TODO: Get tags fron api
    protected readonly Dictionary<int, string> TagNames = new()
    {
        { 1, "Meta" },
        { 2, "Fun" }
    };
    
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    [Inject] private IUserService _userService { get; set; } = null!;

    protected override void OnParametersSet()
    {
        CodeText = DeckItem.Code;
    }
    
    private string? _cardsLoadedForCode;
    private string? _creatorUuidLoaded;
    
    protected override async Task OnParametersSetAsync()
    {
        // Load creator name (once per UUID)
        if (!string.IsNullOrWhiteSpace(DeckItem.CreatorUui))
        {
            if (_creatorUuidLoaded != DeckItem.CreatorUui)
            {
                CreatorName = null;
                if (Guid.TryParse(DeckItem.CreatorUui, out Guid creatorGuid))
                {
                    try
                    {
                        CreatorName = await _userService.GetNameByUuidAsync(creatorGuid);
                    }
                    catch
                    {
                        // ignore errors, leave name null
                    }
                }
                _creatorUuidLoaded = DeckItem.CreatorUui;
            }
        }
        else
        {
            CreatorName = null;
            _creatorUuidLoaded = null;
        }
        
        // Load highlighted cards
        if (string.IsNullOrWhiteSpace(DeckItem.Code) || DeckItem.HighlightedCards.Count == 0)
        {
            HighlightedTCGPCards = [];
            _cardsLoadedForCode = null;
            return;
        }
        
        if (_cardsLoadedForCode == DeckItem.Code && HighlightedTCGPCards.Count > 0)
            return;
        
        List<TCGPCardRequest> tcgpCardRequests = DeckItem.HighlightedCards
            .Select(c => new TCGPCardRequest(c.CollectionCode, c.CollectionNumber))
            .ToList();

        TCGPCardsRequest tcgpCardsRequest = new(tcgpCardRequests);
        List<TCGPCard> cards;
        try
        {
            cards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(tcgpCardsRequest, loadThumbnail:true);
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled (navigation/logout). Safely ignore and show no highlighted cards.
            cards = [];
        }
        catch (System.Net.Http.HttpRequestException)
        {
            // Network/API error (e.g., 5xx). Do not crash the UI on logout or transient failures.
            cards = [];
        }
        catch
        {
            // Any other unexpected error — fail soft.
            cards = [];
        }
        
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