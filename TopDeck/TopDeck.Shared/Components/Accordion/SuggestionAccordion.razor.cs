using Microsoft.AspNetCore.Components;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;

namespace TopDeck.Shared.Components;

public class SuggestionAccordionBase : PresenterBase
{
    #region Statements

    [Parameter, EditorRequired] public required DeckDetailsSuggestion Suggestion { get; set; }
    [Parameter, EditorRequired] public required string Width { get; set; } = "100%";
    [Parameter, EditorRequired] public required string Height { get; set; } = "45px";
    [Parameter, EditorRequired] public required string FontSize { get; set; } = "1em";

    protected bool IsOpen;
    protected IReadOnlyList<TCGPCard> AddedCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> RemovedCards { get; set; } = [];
    
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;
    
    protected override async Task OnParametersSetAsync()
    {
        AddedCards = await LoadCardsAsync(Suggestion.AddedCards);
        RemovedCards = await LoadCardsAsync(Suggestion.RemovedCards);
    }

    #endregion

    #region Methods

    protected void Toggle()
    {
        IsOpen = !IsOpen;
    }
    
    
    protected static IEnumerable<TCGPCard> SortCards(IEnumerable<TCGPCard> cards)
    {
        return cards
            .OrderBy(GetCardPrimaryTypeIndex)
            .ThenBy(c => c.Collection.Code)
            .ThenBy(c => c.CollectionNumber);
    }

    private async Task<IReadOnlyList<TCGPCard>> LoadCardsAsync(IEnumerable<DeckDetailsCard> cards)
    {
        List<TCGPCardRequest> requests = cards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
            .ToList();

        if (requests.Count <= 0)
            return [];

        return await _tcgpCardRequester.GetTCGPCardsByRequestAsync(new TCGPCardsRequest(requests), loadThumbnail: true);
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

    #endregion
}