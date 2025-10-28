using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Contracts.DTO;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;
using TopDeck.Shared.UIStore.States.Tags;
using DomainTag = TopDeck.Domain.Models.Tag;
using DomainDeckDetails = TopDeck.Domain.Models.DeckDetails;

namespace TopDeck.Client.Pages;

public class DeckDetailsEditPagePresenter : PresenterBase
{
    #region Enums
    
    protected enum Tab
    {
        Cards,
        Overview
    }

    #endregion
    
    #region Statements
    
    protected const int MAX_CARDS_IN_DECK = 20;
    protected const int MAX_IDENTICAL_CARDS_IN_DECK = 2;
    private const int MAX_CARDS_DURING_BUILD_DECK = 30;
    private const int MAX_HIGHLIGHT_CARDS = 3;
    
    
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
    
    protected string DeckName { get; set; } = string.Empty;
    
    protected List<TCGPCard> TCGPCards { get; set; } = [];
    protected List<TCGPCard> TCGPCardsCache { get; set; } = [];
    
    // TODO: merge to one list with a IsHighlighted property, add DeckCard id for cards to find the correct highlighted cards
    protected List<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    protected List<TCGPCard> TCGPHighlightedCardsCache { get; set; } = [];
    private Dictionary<string, bool> _tcgpHighlightedCardsMapping { get; set; } = new();
    
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> FilteredTCGPAllCards { get; private set; } = [];

    // Filter popup state for TCGPAllCards
    protected bool IsFilterOpen { get; private set; }
    protected bool IsOrderOpen { get; private set; }
    protected string? SearchInput { get; set; }
    protected string? OrderByInput { get; set; } = "collectionCode"; // name | collectionCode | typeName
    protected bool AscInput { get; set; } = true; // default A-Z
    
    protected List<string> AllTypeNames { get; private set; } = [];
    protected List<string> AllCollectionCodes { get; private set; } = [];
    protected List<string> AllPokemonTypeNames { get; private set; } = [];
    protected HashSet<string> SelectedTypeNames { get; private set; } = [];
    protected HashSet<string> SelectedCollectionCodes { get; private set; } = [];
    protected HashSet<string> SelectedPokemonTypeNames { get; private set; } = [];
    
    protected List<int> EnergyIds { get; set; } = [];
    protected List<int> EnergyIdsCache { get; set; } = [];
    
    
    protected int TotalCardsInDeck => TCGPCards.Count;
    protected bool IsDeckModified => !SortCards(TCGPCards).SequenceEqual(SortCards(TCGPCardsCache));
    
    protected Tab CurrentTab { get; private set; } = Tab.Cards;
    protected bool IsEditing { get; private set; }
    protected string? SelectedCardId { get; private set; }
    
    protected bool IsPickingHighlightCards { get; private set; }
    protected bool IsPickingEnergies { get; private set; }
    protected bool IsPickingTags { get; private set; }
    
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private IDeckDetailsService _deckDetailsService { get; set; } = null!;
    [Inject] private ITagService _tagService { get; set; } = null!;

    [Parameter] public string? DeckCode { get; set; }
    
    private int? _deckId;
    private TCGPCard? _selectedCard { get; set; }

    protected IReadOnlyList<DomainTag> AvailableTags => UIStore.GetState<TagsState>().Tags;

    protected List<int> TagIds { get; set; } = [];
    protected List<int> TagIdsCache { get; set; } = [];
    
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

        // Build distinct filters lists from all cards
        AllTypeNames = TCGPAllCards.Select(c => c.Type.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
        AllCollectionCodes = TCGPAllCards.Select(c => c.Collection.Code).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
        AllPokemonTypeNames = TCGPAllCards.OfType<TCGPPokemonCard>().Select(c => c.PokemonType.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
        ApplyTCGPCardsFilter();

        // Load Tags into UIStore if empty
        TagsState tagsState = UIStore.GetState<TagsState>();
        if (tagsState.Tags.Count == 0)
        {
            IReadOnlyList<DomainTag> tags = await _tagService.GetAllAsync();
            UIStore.Dispatch(new SetTagsAction(tags));
        }

        if (!string.IsNullOrWhiteSpace(DeckCode))
        {
            DomainDeckDetails? existing = await _deckDetailsService.GetByCodeAsync(DeckCode);
            if (existing is null)
            {
                await GoBackAsync();
                return;
            }

            _deckId = existing.Id;
            DeckName = existing.Name;
            EnergyIds = existing.EnergyIds.ToList();
            EnergyIdsCache = existing.EnergyIds.ToList();
            TagIds = existing.TagIds.ToList();
            TagIdsCache = existing.TagIds.ToList();

            // Map highlighted cards (ensure unique by card key, max 3)
            TCGPHighlightedCards = existing.Cards
                .Where(hc => hc.IsHighlighted)
                .Select(hc => TCGPAllCards.FirstOrDefault(c => c.Collection.Code == hc.CollectionCode && c.CollectionNumber == hc.CollectionNumber))
                .Where(c => c is not null)
                .Cast<TCGPCard>()
                .DistinctBy(GetCardKey)
                .Take(3)
                .ToList();
            
            TCGPHighlightedCardsCache = new List<TCGPCard>(TCGPHighlightedCards);
            
            _tcgpHighlightedCardsMapping = TCGPHighlightedCards.ToDictionary<TCGPCard, string, bool>(c => GetCardKey(c), c => true);

            // Build deck cards list with duplicates, but cap identical copies to the allowed maximum
            var groupedByCard = existing.Cards
                .GroupBy(dc => new { dc.CollectionCode, dc.CollectionNumber })
                .Select(g => new { g.Key.CollectionCode, g.Key.CollectionNumber, Count = Math.Min(g.Count(), MAX_IDENTICAL_CARDS_IN_DECK) })
                .ToList();

            List<TCGPCard> loaded = new();
            foreach (var g in groupedByCard)
            {
                TCGPCard? match = TCGPAllCards.FirstOrDefault(c => c.Collection.Code == g.CollectionCode && c.CollectionNumber == g.CollectionNumber);
                if (match is null) continue;
                for (int i = 0; i < g.Count; i++) loaded.Add(match);
            }
            TCGPCards = SortCards(loaded).ToList();

            TCGPCardsCache = new List<TCGPCard>(TCGPCards);
        }
    }

    #endregion

    #region Methods

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
            var pokemonFiltered = query.OfType<TCGPPokemonCard>().Where(p => SelectedPokemonTypeNames.Contains(p.PokemonType.Name));
            var nonPokemon = query.Where(c => c is not TCGPPokemonCard);
            query = nonPokemon.Concat<TCGPCard>(pokemonFiltered);
        }
        // Sorting
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

    protected void ToggleCollectionCode(string code)
    {
        if (SelectedCollectionCodes.Contains(code)) SelectedCollectionCodes.Remove(code); else SelectedCollectionCodes.Add(code);
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
        AscInput = true; // A-Z by default
        ApplyTCGPCardsFilter();
    }

    protected bool IsOrderSelected(string key) => string.Equals(OrderByInput, key, StringComparison.OrdinalIgnoreCase);

    protected void SelectOrder(string key)
    {
        // Selecting the same order keeps AscInput as is; selecting a new order resets to ascending by default
        if (!IsOrderSelected(key))
        {
            OrderByInput = key;
            AscInput = true;
            ApplyTCGPCardsFilter();
        }
    }

    protected void ToggleOrderDirection(string key)
    {
        // If toggling direction on a non-selected order, select it first and default to ascending, then flip
        if (!IsOrderSelected(key))
        {
            OrderByInput = key;
            AscInput = true;
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

    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }
    
    protected void SetPickingHighlightCardsMode()
    {
        // ensure mapping reflects current highlighted list before displaying picking UI
        RebuildHighlightedMapping();
        IsPickingHighlightCards = true;
    }
    
    protected void ExitPickingHighlightCardsMode()
    {
        IsPickingHighlightCards = false;
    }
    
    protected void SetPickingEnergiesMode()
    {
        IsPickingEnergies = true;
    }
    
    protected void ExitPickingEnergiesMode()
    {
        IsPickingEnergies = false;
    }
    
    protected void SetPickingTagsMode()
    {
        IsPickingTags = true;
    }
    
    protected void ExitPickingTagsMode()
    {
        IsPickingTags = false;
    }
    
    protected void AddToHighlightCards(TCGPCard card)
    {
        string key = GetCardKey(card);
        // Toggle off if already selected (by logical key)
        if (TCGPHighlightedCards.Any(c => GetCardKey(c) == key))
        {
            TCGPHighlightedCards.RemoveAll(c => GetCardKey(c) == key);
            _tcgpHighlightedCardsMapping[key] = false;
            return;
        }
        
        if (TCGPHighlightedCards.Count >= MAX_HIGHLIGHT_CARDS)
            return;
        
        TCGPHighlightedCards.Add(card);
        _tcgpHighlightedCardsMapping[key] = true;
    }
    
    protected bool IsCardInHighlightCards(TCGPCard card)
    {
        string key = GetCardKey(card);
        return _tcgpHighlightedCardsMapping.ContainsKey(key) && _tcgpHighlightedCardsMapping[key];
    }
    
    protected void ToggleEnergyType(int energyId)
    {
        if (EnergyIds.Contains(energyId))
        {
            EnergyIds.Remove(energyId);
            return;
        }
        
        if (EnergyIds.Count >= 3)
            return;
        
        EnergyIds.Add(energyId);
    }

    protected bool IsTagSelected(int tagId)
    {
        return TagIds.Contains(tagId);
    }

    protected void ToggleTag(int tagId)
    {
        if (TagIds.Contains(tagId))
        {
            TagIds.Remove(tagId);
        }
        else
        {
            TagIds.Add(tagId);
        }
    }
    
    protected bool IsEnergySelected(int energyId)
    {
        return EnergyIds.Contains(energyId);
    }

    protected void SetEditMode()
    {
        IsEditing = true;
    }
    
    protected async Task ExitEditMode()
    {
        IsEditing = false;
        
        if (TCGPHighlightedCards.Count == 0)
        {
            TCGPHighlightedCards = TCGPCards
                .DistinctBy(c => new { c.Collection.Code, c.CollectionNumber })
                .Take(3)
                .ToList();
        }
        
        if (EnergyIds.Count == 0)
        {
            TCGPCardsRequest energyRequest = new(
                TCGPCards
                    .DistinctBy(c => new { c.Collection.Code, c.CollectionNumber })
                    .Select(c => new TCGPCardRequest(c.Collection.Code, c.CollectionNumber))
                    .ToList()
            );
            
            List<int> energies = await _tcgpCardRequester.GetDeckPokemonTypesAsync(energyRequest);
            EnergyIds = energies.Take(3).ToList();
        }
    }
    
    protected string GetCardElementId(TCGPCard c)
    {
        return $"card-{c.Collection.Code}-{c.CollectionNumber}";
    }
    
    protected async Task SelectCard(string uniqueId, TCGPCard card)
    {
        SelectedCardId = uniqueId;
        _selectedCard = card;
        
        string id = GetCardElementId(card);
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("TopDeck.scrollCardIntoView", id);
    }

    protected void AddToDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        if (TotalCardsInDeck >= MAX_CARDS_DURING_BUILD_DECK)
            return;
        
        int existingCountForName = TCGPCards.Count(c => c.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));
        if (existingCountForName >= MAX_IDENTICAL_CARDS_IN_DECK)
            return;

        TCGPCards.Add(card);
        TCGPCards = SortCards(TCGPCards).ToList();
    }
    
    protected void RemoveOneFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        int index = TCGPCards.FindIndex(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
        if (index >= 0)
        {
            // remove one occurrence from deck
            TCGPCards.RemoveAt(index);
            // if no more copies of this card remain in the deck, unhighlight it
            string key = GetCardKey(card);
            bool stillInDeck = TCGPCards.Any(c => GetCardKey(c) == key);
            if (!stillInDeck)
            {
                TCGPHighlightedCards.RemoveAll(c => GetCardKey(c) == key);
                _tcgpHighlightedCardsMapping[key] = false;
            }
        }
    }
    
    protected void RemoveFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        // remove all copies from deck
        TCGPCards.RemoveAll(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
        // unhighlight if no copies remain
        string key = GetCardKey(card);
        bool stillInDeck = TCGPCards.Any(c => GetCardKey(c) == key);
        if (!stillInDeck)
        {
            TCGPHighlightedCards.RemoveAll(c => GetCardKey(c) == key);
            _tcgpHighlightedCardsMapping[key] = false;
        }
    }
    
    protected void RemoveFromDeckAt(int index)
    {
        if (index < 0 || index >= TCGPCards.Count)
            return;
        
        // capture the card to evaluate highlight state after removal
        TCGPCard removed = TCGPCards[index];
        string key = GetCardKey(removed);
        
        TCGPCards.RemoveAt(index);
        
        // if no more copies remain, unhighlight it
        bool stillInDeck = TCGPCards.Any(c => GetCardKey(c) == key);
        if (!stillInDeck)
        {
            TCGPHighlightedCards.RemoveAll(c => GetCardKey(c) == key);
            _tcgpHighlightedCardsMapping[key] = false;
        }
        
        if (SelectedCardId is not null && _selectedCard is not null)
        {
            // clear selection if it no longer matches
            SelectedCardId = null;
            _selectedCard = null;
        }
    }
    
    protected int GetCardQuantityInDeck(TCGPCard card)
    {
        return TCGPCards.Count(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
    }
    
    protected async Task Cancel()
    {
        await GoBackAsync();
    }
    
    protected async Task Save()
    {
        if (TotalCardsInDeck != MAX_CARDS_IN_DECK)
            return;
        
        if (TCGPHighlightedCards.Count == 0)
        {
            TCGPHighlightedCards = TCGPCards
                .DistinctBy(c => new { c.Collection.Code, c.CollectionNumber })
                .Take(3)
                .ToList();
        }
        
        if (EnergyIds.Count == 0)
        {
            TCGPCardsRequest energyRequest = new(
                TCGPCards
                    .DistinctBy(c => new { c.Collection.Code, c.CollectionNumber })
                    .Select(c => new TCGPCardRequest(c.Collection.Code, c.CollectionNumber))
                    .ToList()
            );
            
            List<int> energies = await _tcgpCardRequester.GetDeckPokemonTypesAsync(energyRequest);
            EnergyIds = energies.Take(3).ToList();
        }

        // Build DTO preserving the user order of highlighted cards by inserting them first
        // Count occurrences of each card in the deck
        Dictionary<string, int> counts = new();
        foreach (TCGPCard c in TCGPCards)
        {
            string k = GetCardKey(c);
            counts[k] = counts.TryGetValue(k, out int v) ? v + 1 : 1;
        }

        List<DeckItemCardInputDTO> dtoCards = new();

        // 1) Add highlighted cards first, in the exact user-selected order, one copy each
        foreach (TCGPCard hc in TCGPHighlightedCards)
        {
            string hk = GetCardKey(hc);
            if (counts.TryGetValue(hk, out int cnt) && cnt > 0)
            {
                dtoCards.Add(new DeckItemCardInputDTO(hc.Collection.Code, hc.CollectionNumber, true));
                counts[hk] = cnt - 1; // consume one copy
            }
        }

        // 2) Append remaining copies of all cards (not highlighted), in the deck list order
        foreach (TCGPCard c in TCGPCards)
        {
            string k = GetCardKey(c);
            if (counts.TryGetValue(k, out int cnt) && cnt > 0)
            {
                dtoCards.Add(new DeckItemCardInputDTO(c.Collection.Code, c.CollectionNumber, false));
                counts[k] = cnt - 1;
            }
        }

        DeckItemInputDTO dto = new(
            UIStore.GetState<AuthenticatedUserState>().Id,
            DeckName,
            dtoCards,
            EnergyIds: EnergyIds,
            TagIds: TagIds
        );

        if (_deckId.HasValue)
        {
            await _deckItemService.UpdateAsync(_deckId.Value, dto);
        }
        else
        {
            await _deckItemService.CreateAsync(dto);
        }
        
        TCGPCardsCache = new List<TCGPCard>(TCGPCards);
        EnergyIdsCache = new List<int>(EnergyIds);
        TagIdsCache = new List<int>(TagIds);
        TCGPHighlightedCardsCache = new List<TCGPCard>(TCGPHighlightedCards);
        await GoBackAsync();
    }
    
    protected bool NothingModified()
    {
        return SortCards(TCGPCards).SequenceEqual(SortCards(TCGPCardsCache))
               && EnergyIds.OrderBy(id => id).SequenceEqual(EnergyIdsCache.OrderBy(id => id))
               && TagIds.OrderBy(id => id).SequenceEqual(TagIdsCache.OrderBy(id => id))
               && TCGPHighlightedCards.Select(GetCardKey).OrderBy(k => k).SequenceEqual(TCGPHighlightedCardsCache.Select(GetCardKey).OrderBy(k => k));
    }
    
    
    private async Task GoBackAsync()
    {
        await JS.InvokeVoidAsync("historyBack", NavigationManager.BaseUri + "decks");
    }
    
    private static IEnumerable<TCGPCard> SortCards(IEnumerable<TCGPCard> cards)
    {
        return cards.OrderBy(c => c.Collection.Code).ThenBy(c => c.CollectionNumber).ThenBy(c => c.Name);
    }

    private static string GetCardKey(TCGPCard c)
    {
        return $"{c.Collection.Code}:{c.CollectionNumber}";
    }

    private void RebuildHighlightedMapping()
    {
        // Recompute the mapping from current highlighted list to avoid stale references/flags
        _tcgpHighlightedCardsMapping = TCGPHighlightedCards
            .ToDictionary<TCGPCard, string, bool>(c => GetCardKey(c), _ => true);
    }

    #endregion
}