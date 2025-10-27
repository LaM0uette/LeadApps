using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models;
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
    protected IReadOnlyList<TCGPCard> FilteredTCGPAllCards { get; private set; } = [];
    protected List<TCGPCard> TCGPSuggestionsCards { get; set; } = [];
    protected List<TCGPCard> TCGPSuggestionsAddedCards { get; set; } = [];
    protected List<TCGPCard> TCGPSuggestionsRemovedCards { get; set; } = [];
    protected string? SelectedCardId { get; private set; }
    private TCGPCard? _selectedCard { get; set; }
    protected int TotalCardsInSuggestion => TCGPSuggestionsCards.Count;

    // Filter popup state for TCGPAllCards (suggestion mode & edit deck if applicable)
    protected bool IsFilterOpen { get; private set; }
    protected bool IsOrderOpen { get; private set; }
    protected string? SearchInput { get; set; }
    protected string? OrderByInput { get; set; } = "collectionCode"; // name | collectionCode | typeName
    protected bool AscInput { get; set; } = true;
    
    protected List<string> AllTypeNames { get; private set; } = [];
    protected List<string> AllCollectionCodes { get; private set; } = [];
    protected List<string> AllPokemonTypeNames { get; private set; } = [];
    protected HashSet<string> SelectedTypeNames { get; private set; } = [];
    protected HashSet<string> SelectedCollectionCodes { get; private set; } = [];
    protected HashSet<string> SelectedPokemonTypeNames { get; private set; } = [];
    
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
    
    protected Mode CurrentMode { get; set; } = Mode.View;
    protected Tab CurrentTab { get; set; } = Tab.Cards;
    
    protected string? AuthenticatedUserUuid;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckDetailsService _deckDetailsService { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    
    protected readonly List<OrderOption> OrderOptions = [];

    protected override void OnInitialized()
    {
        OrderOptions.Add(new OrderOption("collectionCode", Localizer.Localize("component.cardOrder.orderBy.collection.text", "Recent"), defaultAsc: true));
        OrderOptions.Add(new OrderOption("name", Localizer.Localize("component.cardOrder.orderBy.name.text", "Name"), defaultAsc: true));
        OrderOptions.Add(new OrderOption("typeName", Localizer.Localize("component.cardOrder.orderBy.type.text", "Likes"), defaultAsc: true));
    }

    protected override async Task OnInitializedAsync()
    {
        TCGPAllCards = await _tcgpCardRequester.GetAllTCGPCardsAsync(loadThumbnail:true);
        AllTypeNames = TCGPAllCards.Select(c => c.Type.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
        AllCollectionCodes = TCGPAllCards.Select(c => c.Collection.Code).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
        AllPokemonTypeNames = TCGPAllCards.OfType<TCGPPokemonCard>().Select(c => c.PokemonType.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
        ApplyTCGPCardsFilter();
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

    private static readonly Dictionary<string, int> _collectionOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A1"] = 1,
        ["A1a"] = 2,
        ["A2"] = 3,
        ["A2a"] = 4,
        ["A2b"] = 5,
        ["A3"] = 6,
        ["A3a"] = 7,
        ["A3b"] = 8,
        ["A4"] = 9,
        ["A4a"] = 10,
        ["A4b"] = 11,
        ["P-A"] = 12
    };

    private static int GetCollectionIndex(string code)
    {
        return _collectionOrder.TryGetValue(code, out int idx) ? idx : int.MaxValue;
    }

    private static readonly Dictionary<string, int> _pokemonTypeOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Grass"] = 1,
        ["Fire"] = 2,
        ["Water"] = 3,
        ["Lightning"] = 4,
        ["Psychic"] = 5,
        ["Fighting"] = 6,
        ["Darkness"] = 7,
        ["Metal"] = 8,
        ["Dragon"] = 9,
        ["Colorless"] = 10
    };

    private static int GetPokemonTypeIndex(string typeName)
    {
        return _pokemonTypeOrder.TryGetValue(typeName, out int idx) ? idx : int.MaxValue;
    }

    private void ApplyTCGPCardsFilter()
    {
        IEnumerable<TCGPCard> query = TCGPAllCards;
        if (!string.IsNullOrWhiteSpace(SearchInput))
        {
            string s = SearchInput.Trim().ToLowerInvariant();
            query = query.Where(c => (c.Name ?? string.Empty).ToLowerInvariant().Contains(s));
        }
        if (SelectedTypeNames.Count > 0)
        {
            query = query.Where(c => SelectedTypeNames.Contains(c.Type.Name));
        }
        if (SelectedCollectionCodes.Count > 0)
        {
            query = query.Where(c => SelectedCollectionCodes.Contains(c.Collection.Code));
        }
        if (SelectedPokemonTypeNames.Count > 0)
        {
            // Apply only to Pokémon cards; leave other card types unaffected
            var pokemonFiltered = query.OfType<TCGPPokemonCard>().Where(p => SelectedPokemonTypeNames.Contains(p.PokemonType.Name));
            var nonPokemon = query.Where(c => c is not TCGPPokemonCard);
            query = nonPokemon.Concat<TCGPCard>(pokemonFiltered);
        }
        bool asc = AscInput;
        switch ((OrderByInput ?? "collectionCode").ToLowerInvariant())
        {
            case "collectioncode":
                query = asc
                    ? query.OrderBy(c => GetCollectionIndex(c.Collection.Code)).ThenBy(c => c.CollectionNumber).ThenBy(c => c.Name)
                    : query.OrderByDescending(c => GetCollectionIndex(c.Collection.Code)).ThenByDescending(c => c.CollectionNumber).ThenByDescending(c => c.Name);
                break;
            case "typename":
                if (asc)
                {
                    // Ascending: Pokémon first, then by Pokémon type index (custom order),
                    // non‑Pokémon ordered by their card type name; tie‑breaker by card name
                    query = query
                        .OrderBy(c => c is not TCGPPokemonCard ? 1 : 0)
                        .ThenBy(c => c is TCGPPokemonCard p ? GetPokemonTypeIndex(p.PokemonType.Name) : int.MaxValue)
                        .ThenBy(c => c is TCGPPokemonCard ? string.Empty : c.Type.Name)
                        .ThenBy(c => c.Name);
                }
                else
                {
                    // Descending: non‑Pokémon first; Pokémon ordered by descending type index,
                    // non‑Pokémon by type name descending; tie‑breaker by name desc
                    query = query
                        .OrderBy(c => c is not TCGPPokemonCard ? 0 : 1)
                        .ThenByDescending(c => c is TCGPPokemonCard p ? GetPokemonTypeIndex(p.PokemonType.Name) : int.MinValue)
                        .ThenByDescending(c => c is TCGPPokemonCard ? string.Empty : c.Type.Name)
                        .ThenByDescending(c => c.Name);
                }
                break;
            case "name":
            default:
                query = asc
                    ? query.OrderBy(c => c.Name).ThenBy(c => GetCollectionIndex(c.Collection.Code)).ThenBy(c => c.CollectionNumber)
                    : query.OrderByDescending(c => c.Name).ThenByDescending(c => GetCollectionIndex(c.Collection.Code)).ThenByDescending(c => c.CollectionNumber);
                break;
        }
        FilteredTCGPAllCards = query.ToList();
    }

    protected void OpenFilter()
    {
        IsFilterOpen = true;
    }

    protected void CloseFilter()
    {
        IsFilterOpen = false;
    }

    protected void OpenOrder()
    {
        IsOrderOpen = true;
    }

    protected void CloseOrder()
    {
        IsOrderOpen = false;
    }

    protected void ToggleTypeName(string name)
    {
        if (SelectedTypeNames.Contains(name)) SelectedTypeNames.Remove(name); else SelectedTypeNames.Add(name);
    }

    protected void ToggleCollectionCode(string collectionCode)
    {
        if (SelectedCollectionCodes.Contains(collectionCode)) SelectedCollectionCodes.Remove(collectionCode); else SelectedCollectionCodes.Add(collectionCode);
    }

    protected void TogglePokemonTypeName(string name)
    {
        if (SelectedPokemonTypeNames.Contains(name)) SelectedPokemonTypeNames.Remove(name); else SelectedPokemonTypeNames.Add(name);
    }

    protected void ResetFilter()
    {
        SearchInput = null;
        SelectedTypeNames.Clear();
        SelectedCollectionCodes.Clear();
        SelectedPokemonTypeNames.Clear();
        ApplyTCGPCardsFilter();
    }

    protected void ApplyFilter()
    {
        ApplyTCGPCardsFilter();
        IsFilterOpen = false;
        StateHasChanged();
    }

    protected void ResetOrder()
    {
        OrderByInput = "collectionCode";
        AscInput = true;
        ApplyTCGPCardsFilter();
    }

    protected bool IsOrderSelected(string key) => string.Equals(OrderByInput, key, StringComparison.OrdinalIgnoreCase);

    protected void SelectOrder(string key)
    {
        if (!IsOrderSelected(key))
        {
            OrderByInput = key;
            var option = OrderOptions.FirstOrDefault(o => string.Equals(o.Key, key, StringComparison.OrdinalIgnoreCase));
            AscInput = option?.DefaultAsc ?? true;
            ApplyTCGPCardsFilter();
        }
    }

    protected void ToggleOrderDirection(string key)
    {
        if (!IsOrderSelected(key))
        {
            OrderByInput = key;
            var option = OrderOptions.FirstOrDefault(o => string.Equals(o.Key, key, StringComparison.OrdinalIgnoreCase));
            AscInput = option?.DefaultAsc ?? true;
        }
        AscInput = !AscInput;
        ApplyTCGPCardsFilter();
    }

    protected void ApplyOrder()
    {
        ApplyTCGPCardsFilter();
        IsOrderOpen = false;
        StateHasChanged();
    }

    #endregion
}