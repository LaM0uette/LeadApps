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
    protected IReadOnlyList<TCGPCard> TCGPCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPAllCards { get; set; } = [];
    
    protected Tab CurrentTab { get; set; } = Tab.Cards;
    protected bool IsEditing { get; set; }
    
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        TCGPCardsRequest deckRequest = new(
            [
                new TCGPCardRequest("A1", 1),
                new TCGPCardRequest("A1", 2),
                new TCGPCardRequest("A1", 3),
                new TCGPCardRequest("A1", 4),
                new TCGPCardRequest("A1", 5),
                new TCGPCardRequest("A1", 6),
                new TCGPCardRequest("A1", 7),
                new TCGPCardRequest("A1", 8),
                new TCGPCardRequest("A1", 9),
                new TCGPCardRequest("A1", 10),
                new TCGPCardRequest("A1", 11),
                new TCGPCardRequest("A1", 12),
                new TCGPCardRequest("A1", 13),
                new TCGPCardRequest("A1", 14),
                new TCGPCardRequest("A1", 15),
                new TCGPCardRequest("A1", 16),
                new TCGPCardRequest("A1", 17)
            ]);
        
        TCGPCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest, loadThumbnail:true);
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

    #endregion
}