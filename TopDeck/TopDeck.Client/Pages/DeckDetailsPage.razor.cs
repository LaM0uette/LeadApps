using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Client.Pages;

public class DeckDetailsPagePresenter : PresenterBase
{
    #region Enums

    protected enum Mode
    {
        View,
        Edit,
        AddSuggestion
    }
    
    protected enum Tab
    {
        Cards,
        Overview,
        Suggestions
    }

    #endregion
    
    #region Statements
    
    protected const int MAX_CARDS_IN_DECK = 20;
    protected const int MAX_IDENTICAL_CARDS_IN_DECK = 2;
    private const int MAX_CARDS_DURING_BUILD_DECK = 30;
    private const int MAX_HIGHLIGHT_CARDS = 3;

    [Parameter, EditorRequired] public required string DeckCode { get; set; }

    protected DeckDetails? DeckDetails;
    protected IReadOnlyList<TCGPCard> TCGPCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    protected List<TCGPCard> TCGPSuggestionsCards { get; set; } = [];
    protected List<TCGPCard> TCGPSuggestionsAddedCards { get; set; } = [];
    protected List<TCGPCard> TCGPSuggestionsRemovedCards { get; set; } = [];
    protected string? SelectedCardId { get; private set; }
    private TCGPCard? _selectedCard { get; set; }
    protected int TotalCardsInSuggestion => TCGPSuggestionsCards.Count;
    
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
    
    protected string? AuthenticatedUserUuid;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckDetailsService _deckDetailsService { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        TCGPAllCards = await _tcgpCardRequester.GetAllTCGPCardsAsync(loadThumbnail:true);
    }

    protected override async Task OnParametersSetAsync()
    {
        DeckDetails? deckDetails = await _deckDetailsService.GetByCodeAsync(DeckCode);
        
        if (deckDetails == null)
        {
            //_navigationManager.NavigateTo("/", true);
            return;
        }
        
        DeckDetails = deckDetails;

        AuthenticatedUserState authenticatedUser = UIStore.GetState<AuthenticatedUserState>();
        if (authenticatedUser.Id > 0)
        {
            AuthenticatedUserUuid = authenticatedUser.Uuid;
        }
        
        List<TCGPCardRequest> tcgpCardRequests = DeckDetails.Cards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
            .ToList();
        
        TCGPCardsRequest deckRequest = new(tcgpCardRequests);
        TCGPCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest);
        
        TCGPHighlightedCards = DeckDetails.Cards
            .Select(hc => TCGPCards.FirstOrDefault(c => c.Collection.Code == hc.CollectionCode && c.CollectionNumber == hc.CollectionNumber && hc.IsHighlighted))
            .Where(c => c is not null)
            .Cast<TCGPCard>()
            .Take(3)
            .ToList();
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
        if (DeckDetails == null) 
            return;
        
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", DeckDetails.Code);
        DeckCode = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        DeckCode = DeckDetails.Code;
        StateHasChanged();
    }
    
    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }
    
    protected void SwitchTab()
    {
        CurrentTab = CurrentTab switch
        {
            Tab.Cards => Tab.Overview,
            Tab.Overview => Tab.Cards,
            _ => CurrentTab
        };
    }
    
    protected void EditDeck()
    {
        if (DeckDetails is null)
            return;
        _navigationManager.NavigateTo($"/decks/{DeckDetails.Code}/edit");
    }


    protected void AddSuggestion()
    {
        TCGPSuggestionsCards = new List<TCGPCard>(TCGPCards);
        TCGPSuggestionsAddedCards = [];
        TCGPSuggestionsRemovedCards = [];
        
        CurrentMode = Mode.AddSuggestion;
    }
    
    protected async Task SelectCard(string uniqueId, TCGPCard card)
    {
        SelectedCardId = uniqueId;
        _selectedCard = card;
        
        string id = GetCardElementId(card);
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("TopDeck.scrollCardIntoView", id);
    }

    protected void AddToSuggestions(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        if (TotalCardsInSuggestion >= MAX_CARDS_DURING_BUILD_DECK)
            return;
        
        int existingCountForName = TCGPSuggestionsCards.Count(c => c.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));
        if (existingCountForName >= MAX_IDENTICAL_CARDS_IN_DECK)
            return;

        TCGPSuggestionsCards.Add(card);
        TCGPSuggestionsAddedCards.Add(card);
        TCGPSuggestionsCards = SortCards(TCGPSuggestionsCards).ToList();
        TCGPSuggestionsAddedCards = SortCards(TCGPSuggestionsAddedCards).ToList();
    }
    
    protected void RemoveOneFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        int index = TCGPSuggestionsCards.FindIndex(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
        if (index >= 0)
        {
            TCGPSuggestionsCards.RemoveAt(index);
            TCGPSuggestionsRemovedCards.Add(card);
        }
    }
    
    protected void RemoveFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        int existingCount = TCGPSuggestionsCards.Count(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
        for (int i = 0; i < existingCount; i++)
        {
            TCGPSuggestionsRemovedCards.Add(card);
        }
        
        TCGPSuggestionsCards.RemoveAll(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
    }
    
    protected void RemoveFromDeckAt(int index)
    {
        if (index < 0 || index >= TCGPSuggestionsCards.Count)
            return;
        
        TCGPSuggestionsCards.RemoveAt(index);
        
        if (SelectedCardId is not null && _selectedCard is not null)
        {
            TCGPSuggestionsRemovedCards.Add(_selectedCard);
            // clear selection if it no longer matches
            SelectedCardId = null;
            _selectedCard = null;
        }
    }
    
    protected string GetCardElementId(TCGPCard c)
    {
        return $"card-{c.Collection.Code}-{c.CollectionNumber}";
    }
    
    protected int GetCardQuantityInSuggestion(TCGPCard card)
    {
        return TCGPSuggestionsCards.Count(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
    }
    
    protected async Task Cancel()
    {
        TCGPSuggestionsCards = new List<TCGPCard>(TCGPCards);
        TCGPSuggestionsAddedCards = [];
        TCGPSuggestionsRemovedCards = [];
        
        CurrentMode = Mode.View;
        await InvokeAsync(StateHasChanged);
    }
    
    protected async Task Save()
    {
        if (TotalCardsInSuggestion != MAX_CARDS_IN_DECK)
            return;

        if (DeckDetails is null)
            return;
        
        DeckSuggestionInputDTO dto = new(
            UIStore.GetState<AuthenticatedUserState>().Id,
            DeckDetails.Id,
            TCGPSuggestionsAddedCards.Select(c => new DeckDetailsCardInputDTO(c.Collection.Code, c.CollectionNumber)).ToList(),
            TCGPSuggestionsRemovedCards.Select(c => new DeckDetailsCardInputDTO(c.Collection.Code, c.CollectionNumber)).ToList(),
            new List<int>(),
            new List<int>()
        );
        
        DeckDetailsSuggestion? createdSuggestion = await _deckDetailsService.CreateSuggestionAsync(dto);
        if (createdSuggestion != null)
        {
            CurrentMode = Mode.View;
            TCGPSuggestionsCards = new List<TCGPCard>(TCGPCards);
            TCGPSuggestionsAddedCards = [];
            TCGPSuggestionsRemovedCards = [];
            
            DeckDetails.Suggestions.Add(createdSuggestion);
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            throw new InvalidOperationException("Failed to create deck suggestion.");
        }
    }
    
    protected bool NothingModified()
    {
        return SortCards(TCGPCards).SequenceEqual(SortCards(TCGPSuggestionsCards));
    }
    
    protected async Task GoBackAsync()
    {
        await JS.InvokeVoidAsync("historyBack", NavigationManager.BaseUri + "decks");
    }
    
    
    private static IEnumerable<TCGPCard> SortCards(IEnumerable<TCGPCard> cards)
    {
        return cards.OrderBy(c => c.Collection.Code).ThenBy(c => c.CollectionNumber).ThenBy(c => c.Name);
    }

    #endregion
}