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
    protected List<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    protected List<TCGPCard> TCGPCards { get; set; } = [];
    protected List<TCGPCard> TCGPCardsCache { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    protected List<int> EnergyIds { get; set; } = [];
    
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

            // Map highlighted cards
            TCGPHighlightedCards = existing.HighlightedCards
                .Select(hc => TCGPAllCards.FirstOrDefault(c => c.Collection.Code == hc.CollectionCode && c.CollectionNumber == hc.CollectionNumber))
                .Where(c => c is not null)
                .Cast<TCGPCard>()
                .Take(3)
                .ToList();

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

    private static IEnumerable<TCGPCard> SortCards(IEnumerable<TCGPCard> cards)
    {
        return cards.OrderBy(c => c.Collection.Code).ThenBy(c => c.CollectionNumber).ThenBy(c => c.Name);
    }

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
        
        // Build card list preserving duplicates and mark only one highlighted per unique card
        List<DeckItemCardInputDTO> cardInputs = new();
        HashSet<string> highlightedAssigned = new();
        foreach (TCGPCard c in TCGPCards)
        {
            string key = c.Collection.Code + "|" + c.CollectionNumber.ToString();
            bool isHighlighted = false;
            if (TCGPHighlightedCards.Any(h => h.Collection.Code == c.Collection.Code && h.CollectionNumber == c.CollectionNumber) && !highlightedAssigned.Contains(key))
            {
                isHighlighted = true;
                highlightedAssigned.Add(key);
            }
            cardInputs.Add(new DeckItemCardInputDTO(c.Collection.Code, c.CollectionNumber, isHighlighted));
        }

        DeckItemInputDTO dto = new(
            UIStore.GetState<AuthenticatedUserState>().Id,
            DeckName,
            cardInputs,
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
        await GoBackAsync();
    }
    
    
    private async Task GoBackAsync()
    {
        await JS.InvokeVoidAsync("historyBack", NavigationManager.BaseUri + "decks");
    }

    #endregion
}