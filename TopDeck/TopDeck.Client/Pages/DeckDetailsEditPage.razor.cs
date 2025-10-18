using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;

namespace TopDeck.Client.Pages;

public class DeckDetailsEditPagePresenter : PresenterBase
{
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

    #endregion

    #region Methods

    //

    #endregion
}