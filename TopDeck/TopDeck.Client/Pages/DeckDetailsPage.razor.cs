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
    
    protected string? CreatorName { get; private set; }
    private string? _creatorUuidLoaded;

    // Deltas of suggestion by card key ("Code:Number") => net delta in [−2..+2]
    private readonly Dictionary<string, int> _suggestionDeltas = new();

    private static string KeyOf(TCGPCard c) => $"{c.Collection.Code}:{c.CollectionNumber}";
    private static (string code, int num) ParseKey(string k)
    {
        var parts = k.Split(':');
        return (parts[0], int.Parse(parts[1]));
    }

    private int GetCountInCurrentDeckForKey(string key)
    {
        var (code, num) = ParseKey(key);
        return TCGPCards.Count(c => c.Collection.Code == code && c.CollectionNumber == num);
    }

    private int ClampDeltaForFeasibility(string key, int delta)
    {
        int inDeck = GetCountInCurrentDeckForKey(key);      // 0..2
        int maxAddFeasible = Math.Max(0, MAX_IDENTICAL_CARDS_IN_DECK - inDeck);
        int maxRemFeasible = Math.Min(MAX_IDENTICAL_CARDS_IN_DECK, inDeck);

        if (delta > 0) return Math.Min(delta, maxAddFeasible);
        if (delta < 0) return -Math.Min(-delta, maxRemFeasible);
        return 0;
    }

    private void BumpAdd(TCGPCard card)
    {
        string k = KeyOf(card);
        int cur = _suggestionDeltas.TryGetValue(k, out int v) ? v : 0;
        int next = Math.Min(cur + 1, MAX_IDENTICAL_CARDS_IN_DECK);
        next = ClampDeltaForFeasibility(k, next);
        if (next == 0) _suggestionDeltas.Remove(k); else _suggestionDeltas[k] = next;
        RebuildSuggestionStateFromDeltas();
    }

    private void BumpRemove(TCGPCard card)
    {
        string k = KeyOf(card);
        int cur = _suggestionDeltas.TryGetValue(k, out int v) ? v : 0;
        int next = Math.Max(cur - 1, -MAX_IDENTICAL_CARDS_IN_DECK);
        next = ClampDeltaForFeasibility(k, next);
        if (next == 0) _suggestionDeltas.Remove(k); else _suggestionDeltas[k] = next;
        RebuildSuggestionStateFromDeltas();
    }

    private void SetRemoveAll(TCGPCard card)
    {
        string k = KeyOf(card);
        int inDeck = GetCountInCurrentDeckForKey(k);
        int next = -Math.Min(inDeck, MAX_IDENTICAL_CARDS_IN_DECK);
        next = ClampDeltaForFeasibility(k, next);
        if (next == 0) _suggestionDeltas.Remove(k); else _suggestionDeltas[k] = next;
        RebuildSuggestionStateFromDeltas();
    }

    private void RebuildSuggestionStateFromDeltas()
    {
        // Start from current deck grouped counts capped at 2
        var grouped = TCGPCards
            .GroupBy(c => new { c.Collection.Code, c.CollectionNumber })
            .ToDictionary(g => $"{g.Key.Code}:{g.Key.CollectionNumber}", g => Math.Min(g.Count(), MAX_IDENTICAL_CARDS_IN_DECK));

        foreach (var kv in _suggestionDeltas)
        {
            string k = kv.Key;
            int d = kv.Value;
            int cur = grouped.TryGetValue(k, out int v) ? v : 0;
            int next = cur + d;
            next = Math.Clamp(next, 0, MAX_IDENTICAL_CARDS_IN_DECK);
            if (next == 0) grouped.Remove(k); else grouped[k] = next;
        }

        // Recompose suggestion result list
        var result = new List<TCGPCard>();
        foreach (var (k, qty) in grouped.OrderBy(x => x.Key))
        {
            var (code, num) = ParseKey(k);
            var card = TCGPAllCards.FirstOrDefault(x => x.Collection.Code == code && x.CollectionNumber == num);
            if (card is null) continue;
            result.AddRange(Enumerable.Repeat(card, qty));
        }
        TCGPSuggestionsCards = SortCards(result).ToList();

        // Rebuild Added/Removed display lists from deltas
        TCGPSuggestionsAddedCards = [];
        TCGPSuggestionsRemovedCards = [];
        foreach (var kv in _suggestionDeltas.OrderBy(x => x.Key))
        {
            var (code, num) = ParseKey(kv.Key);
            var card = TCGPAllCards.FirstOrDefault(x => x.Collection.Code == code && x.CollectionNumber == num);
            if (card is null) continue;
            if (kv.Value > 0) TCGPSuggestionsAddedCards.AddRange(Enumerable.Repeat(card, kv.Value));
            if (kv.Value < 0) TCGPSuggestionsRemovedCards.AddRange(Enumerable.Repeat(card, -kv.Value));
        }
    }

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
    [Inject] private IUserService _userService { get; set; } = null!;
    
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
        AllPokemonTypeNames = TCGPAllCards
                    .OfType<TCGPPokemonCard>()
                    .Select(c => c.PokemonType.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(GetPokemonTypeIndex)
                    .ToList();
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
        
        // Load creator name (once per UUID)
        if (!string.IsNullOrWhiteSpace(DeckDetails.CreatorUuid))
        {
            if (_creatorUuidLoaded != DeckDetails.CreatorUuid)
            {
                CreatorName = null;
                if (Guid.TryParse(DeckDetails.CreatorUuid, out Guid creatorGuid))
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
                _creatorUuidLoaded = DeckDetails.CreatorUuid;
            }
        }
        else
        {
            CreatorName = null;
            _creatorUuidLoaded = null;
        }

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
        _suggestionDeltas.Clear();
        
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
        BumpAdd(card);
    }
    
    protected void RemoveOneFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        BumpRemove(card);
    }
    
    protected void RemoveFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        SetRemoveAll(card);
    }
    
    protected void RemoveFromDeckAt(int index)
    {
        if (index < 0 || index >= TCGPSuggestionsCards.Count)
            return;
        
        var removed = TCGPSuggestionsCards[index];
        BumpRemove(removed);
        
        if (SelectedCardId is not null && _selectedCard is not null)
        {
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
        _suggestionDeltas.Clear();
        
        CurrentMode = Mode.View;
        await InvokeAsync(StateHasChanged);
    }
    
    protected async Task Save()
    {
        if (TotalCardsInSuggestion != MAX_CARDS_IN_DECK)
            return;

        if (DeckDetails is null)
            return;
        
        // Build DTO from normalized deltas
        var added = _suggestionDeltas.Where(kv => kv.Value > 0)
            .SelectMany(kv =>
            {
                var (code, num) = ParseKey(kv.Key);
                return Enumerable.Repeat(new DeckDetailsCardInputDTO(code, num), kv.Value);
            }).ToList();
        var removed = _suggestionDeltas.Where(kv => kv.Value < 0)
            .SelectMany(kv =>
            {
                var (code, num) = ParseKey(kv.Key);
                return Enumerable.Repeat(new DeckDetailsCardInputDTO(code, num), -kv.Value);
            }).ToList();
        
        DeckSuggestionInputDTO dto = new(
            UIStore.GetState<AuthenticatedUserState>().Id,
            DeckDetails.Id,
            added,
            removed,
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
            _suggestionDeltas.Clear();
            
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
    
    
    protected static IEnumerable<TCGPCard> SortCards(IEnumerable<TCGPCard> cards)
    {
        return cards
            .OrderBy(GetCardPrimaryTypeIndex)
            .ThenBy(c => c.Collection.Code)
            .ThenBy(c => c.CollectionNumber);
    }

    private static int GetCardPrimaryTypeIndex(TCGPCard c)
    {
        string name = c.Type?.Name?.Trim() ?? string.Empty;
        return name.Equals("Pokemon", StringComparison.OrdinalIgnoreCase) || name.Equals("Pokémon", StringComparison.OrdinalIgnoreCase) ? 1
            : name.Equals("Fossil", StringComparison.OrdinalIgnoreCase) || name.Equals("Fossile", StringComparison.OrdinalIgnoreCase) ? 2
            : name.Equals("Item", StringComparison.OrdinalIgnoreCase) || name.Equals("Objet", StringComparison.OrdinalIgnoreCase) ? 3
            : name.Equals("Tool", StringComparison.OrdinalIgnoreCase) || name.Equals("Outil", StringComparison.OrdinalIgnoreCase) ? 4
            : name.Equals("Supporter", StringComparison.OrdinalIgnoreCase) ? 5
            : name.Equals("Stadium", StringComparison.OrdinalIgnoreCase) || name.Equals("Stade", StringComparison.OrdinalIgnoreCase) ? 6
            : 7;
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
        ["B1"] = 12,
        ["B1a"] = 13,
        ["B2"] = 14,
        ["B2a"] = 15,
        ["P-A"] = 16,
        ["P-B"] = 17
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
                    ? query.OrderBy(GetCardPrimaryTypeIndex).ThenBy(c => GetCollectionIndex(c.Collection.Code)).ThenBy(c => c.CollectionNumber)
                    : query.OrderByDescending(GetCardPrimaryTypeIndex).ThenByDescending(c => GetCollectionIndex(c.Collection.Code)).ThenByDescending(c => c.CollectionNumber);
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