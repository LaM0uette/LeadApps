using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Contracts.DTO;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;
using TopDeck.Domain.Models;

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
    
    protected List<int> EnergyIds { get; set; } = [];
    protected List<int> EnergyIdsCache { get; set; } = [];
    
    
    protected int TotalCardsInDeck => TCGPCards.Count;
    protected bool IsDeckModified => !SortCards(TCGPCards).SequenceEqual(SortCards(TCGPCardsCache));
    
    protected Tab CurrentTab { get; private set; } = Tab.Cards;
    protected bool IsEditing { get; private set; }
    protected string? SelectedCardId { get; private set; }
    
    protected bool IsPickingHighlightCards { get; private set; }
    protected bool IsPickingEnergies { get; private set; }
    
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private IDeckDetailsService _deckDetailsService { get; set; } = null!;

    [Parameter] public string? DeckCode { get; set; }
    
    private int? _deckId;
    private TCGPCard? _selectedCard { get; set; }

    protected override async Task OnInitializedAsync()
    {
        TCGPAllCards = await _tcgpCardRequester.GetAllTCGPCardsAsync(loadThumbnail:true);

        if (!string.IsNullOrWhiteSpace(DeckCode))
        {
            DeckDetails? existing = await _deckDetailsService.GetByCodeAsync(DeckCode);
            if (existing is null)
            {
                await GoBackAsync();
                return;
            }

            _deckId = existing.Id;
            DeckName = existing.Name;
            EnergyIds = existing.EnergyIds.ToList();
            EnergyIdsCache = existing.EnergyIds.ToList();

            // Map highlighted cards
            TCGPHighlightedCards = existing.Cards
                .Select(hc => TCGPAllCards.FirstOrDefault(c => c.Collection.Code == hc.CollectionCode && c.CollectionNumber == hc.CollectionNumber && hc.IsHighlighted))
                .Where(c => c is not null)
                .Cast<TCGPCard>()
                .Take(3)
                .ToList();
            
            TCGPHighlightedCardsCache = new List<TCGPCard>(TCGPHighlightedCards);
            
            _tcgpHighlightedCardsMapping = TCGPHighlightedCards.ToDictionary<TCGPCard, string, bool>(c => GetCardKey(c), c => true);

            // Build deck cards list with duplicates
            TCGPCards = existing.Cards
                .Select(dc => TCGPAllCards.FirstOrDefault(c => c.Collection.Code == dc.CollectionCode && c.CollectionNumber == dc.CollectionNumber))
                .Where(c => c is not null)
                .Cast<TCGPCard>()
                .ToList();
            TCGPCards = SortCards(TCGPCards).ToList();

            TCGPCardsCache = new List<TCGPCard>(TCGPCards);
        }
    }

    #endregion

    #region Methods

    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }
    
    protected void SetPickingHighlightCardsMode()
    {
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
    
    protected bool IsEnergySelected(int energyId)
    {
        return EnergyIds.Contains(energyId);
    }

    protected void SetEditMode()
    {
        IsEditing = true;
    }
    
    protected void ExitEditMode()
    {
        IsEditing = false;
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
            TCGPCards.RemoveAt(index);
        }
    }
    
    protected void RemoveFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCard = null;
        
        TCGPCards.RemoveAll(c => c.Collection.Code == card.Collection.Code && c.CollectionNumber == card.CollectionNumber);
    }
    
    protected void RemoveFromDeckAt(int index)
    {
        if (index < 0 || index >= TCGPCards.Count)
            return;
        
        TCGPCards.RemoveAt(index);
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
            TagIds: []
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
        TCGPHighlightedCardsCache = new List<TCGPCard>(TCGPHighlightedCards);
        await GoBackAsync();
    }
    
    protected bool NothingModified()
    {
        return SortCards(TCGPCards).SequenceEqual(SortCards(TCGPCardsCache))
               && EnergyIds.OrderBy(id => id).SequenceEqual(EnergyIdsCache.OrderBy(id => id))
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

    #endregion
}