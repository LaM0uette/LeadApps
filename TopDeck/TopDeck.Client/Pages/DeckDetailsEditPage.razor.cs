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
        TCGPCardRef cardRef = new(card.Name, card.Collection.Code, card.CollectionNumber, card.ImageUrl ?? string.Empty);

        int existingCountForName = TCGPCards
            .Where(kv => kv.Key.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase))
            .Sum(kv => kv.Value);

        if (existingCountForName >= 2)
            return;

        if (TCGPCards.TryAdd(cardRef, 1))
            return;

        TCGPCards[cardRef]++;
    }

    #endregion
}