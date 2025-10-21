using Microsoft.AspNetCore.Components;
using TCGPCardRequester;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;

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
    
    private const int MAX_CARDS_IN_DECK = 20;
    protected const int MAX_IDENTICAL_CARDS_IN_DECK = 2;
    private const int MAX_CARDS_DURING_BUILD_DECK = 30;
    
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
    
    protected string DeckName { get; set; } = "Nom du Deck";
    protected IReadOnlyList<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    protected Dictionary<TCGPCardRef, int> TCGPCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    
    protected Tab CurrentTab { get; set; } = Tab.Cards;
    protected bool IsEditing { get; set; }
    
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        TCGPAllCards = await _tcgpCardRequester.GetAllTCGPCardsAsync(loadThumbnail:true);
    }

    #endregion

    #region Methods

    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }

    protected void SetEditMode()
    {
        IsEditing = true;
    }

    protected void AddToDeck(TCGPCard card)
    {
        if (TCGPCards.Count >= MAX_CARDS_DURING_BUILD_DECK)
            return;
        
        TCGPCardRef cardRef = new(card.Name, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);

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
        TCGPCardRef cardRef = new(card.Name, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);

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
        TCGPCardRef cardRef = new(card.Name, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);
        TCGPCards.Remove(cardRef);
    }
    
    protected int GetCardQuantityInDeck(TCGPCard card)
    {
        TCGPCardRef cardRef = new(card.Name, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);
        return TCGPCards.GetValueOrDefault(cardRef, 0);
    }

    #endregion
}