using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Contracts.DTO;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;
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
    protected List<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    protected Dictionary<TCGPCardRef, int> TCGPCards { get; set; } = [];
    protected Dictionary<TCGPCardRef, int> TCGPCardsCache { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    protected List<int> EnergyIds { get; set; } = [];
    
    protected int TotalCardsInDeck => TCGPCards.Values.Sum();
    protected bool IsDeckModified => !TCGPCards.OrderBy(kv => kv.Key.Name).SequenceEqual(TCGPCardsCache.OrderBy(kv => kv.Key.Name));
    
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
    private TCGPCardRef? _selectedCardRef { get; set; }

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

            // Map highlighted cards
            TCGPHighlightedCards = existing.HighlightedCards
                .Select(hc => TCGPAllCards.FirstOrDefault(c => c.Collection.Code == hc.CollectionCode && c.CollectionNumber == hc.CollectionNumber))
                .Where(c => c is not null)
                .Cast<TCGPCard>()
                .Take(3)
                .ToList();

            // Build deck cards with quantities
            TCGPCards = existing.Cards
                .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
                .Select(g =>
                {
                    TCGPCard? card = TCGPAllCards.FirstOrDefault(c => c.Collection.Code == g.Key.CollectionCode && c.CollectionNumber == g.Key.CollectionNumber);
                    if (card is null) return (Ref: (TCGPCardRef?)null, Count: 0);
                    TCGPCardRef cardRef = new(card.Name, card.Type.Id, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);
                    return (Ref: (TCGPCardRef?)cardRef, Count: g.Count());
                })
                .Where(x => x.Ref is not null && x.Count > 0)
                .ToDictionary(x => x.Ref!, x => x.Count);

            TCGPCardsCache = new Dictionary<TCGPCardRef, int>(TCGPCards);
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
        if (TCGPHighlightedCards.Remove(card))
            return;
        
        if (TCGPHighlightedCards.Count >= MAX_HIGHLIGHT_CARDS)
            return;
        
        TCGPHighlightedCards.Add(card);
    }
    
    protected bool IsCardInHighlightCards(TCGPCard card)
    {
        return TCGPHighlightedCards.Contains(card);
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
    
    protected async Task SelectCard(string uniqueId, TCGPCardRef cardRef)
    {
        SelectedCardId = uniqueId;
        _selectedCardRef = cardRef;
        
        TCGPCard? card = TCGPAllCards.FirstOrDefault(c => c.Collection.Code == cardRef.CollectionCode && c.CollectionNumber == cardRef.CollectionNumber);
        if (card is null) 
            return;
        
        string id = GetCardElementId(card);
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("TopDeck.scrollCardIntoView", id);
    }

    protected void AddToDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCardRef = null;
        
        if (TotalCardsInDeck >= MAX_CARDS_DURING_BUILD_DECK)
            return;
        
        TCGPCardRef cardRef = new(card.Name, card.Type.Id, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);

        int existingCountForName = TCGPCards
            .Where(kv => kv.Key.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase))
            .Sum(kv => kv.Value);

        if (existingCountForName >= MAX_IDENTICAL_CARDS_IN_DECK)
            return;

        if (TCGPCards.TryAdd(cardRef, 1))
            return;

        TCGPCards[cardRef]++;
    }
    
    protected void RemoveOneFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCardRef = null;
        
        TCGPCardRef cardRef = new(card.Name, card.Type.Id, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);

        if (!TCGPCards.TryGetValue(cardRef, out int quantity))
            return;

        if (quantity <= 1)
        {
            TCGPCards.Remove(cardRef);
            return;
        }

        TCGPCards[cardRef]--;
    }
    
    protected void RemoveFromDeck(TCGPCard card)
    {
        SelectedCardId = null;
        _selectedCardRef = null;
        
        TCGPCardRef cardRef = new(card.Name, card.Type.Id, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);
        TCGPCards.Remove(cardRef);
    }
    
    protected void RemoveFromDeckRef(TCGPCardRef cardRef)
    {
        if (!TCGPCards.TryGetValue(cardRef, out int quantity)) 
            return;
        
        if (quantity <= 1)
        {
            TCGPCards.Remove(cardRef);
        }
        else
        {
            TCGPCards[cardRef] = quantity - 1;
        }
        
        if (_selectedCardRef == cardRef && !TCGPCards.ContainsKey(cardRef))
        {
            SelectedCardId = null;
            _selectedCardRef = null;
        }
    }
    
    protected int GetCardQuantityInDeck(TCGPCard card)
    {
        TCGPCardRef cardRef = new(card.Name, card.Type.Id, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);
        return TCGPCards.GetValueOrDefault(cardRef, 0);
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
                .Take(3)
                .Select(kv => TCGPAllCards.First(c => c.Collection.Code == kv.Key.CollectionCode && c.CollectionNumber == kv.Key.CollectionNumber))
                .ToList();
        }
        
        if (EnergyIds.Count == 0)
        {
            TCGPCardsRequest energyRequest = new(
                TCGPCards
                    .Select(kv => new TCGPCardRequest(kv.Key.CollectionCode, kv.Key.CollectionNumber))
                    .Distinct()
                    .ToList()
            );
            
            List<int> energies = await _tcgpCardRequester.GetDeckPokemonTypesAsync(energyRequest);
            EnergyIds = energies.Take(3).ToList();
        }
        
        DeckItemInputDTO dto = new(
            UIStore.GetState<AuthenticatedUserState>().Id,
            DeckName,
            TCGPCards.Select(kv => new DeckItemCardInputDTO(
                kv.Key.CollectionCode,
                kv.Key.CollectionNumber,
                TCGPHighlightedCards.Any(c => c.Collection.Code == kv.Key.CollectionCode && c.CollectionNumber == kv.Key.CollectionNumber)
                )).ToList(),
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
        
        TCGPCardsCache = new Dictionary<TCGPCardRef, int>(TCGPCards);
        await GoBackAsync();
    }
    
    
    private async Task GoBackAsync()
    {
        await JS.InvokeVoidAsync("historyBack", NavigationManager.BaseUri + "decks");
    }

    #endregion
}