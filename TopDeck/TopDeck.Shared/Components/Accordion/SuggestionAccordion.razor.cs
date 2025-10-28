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
        List<TCGPCardRequest> addedCardRequests = [];
        addedCardRequests.AddRange(Suggestion.AddedCards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
        );

        List<TCGPCardRequest> removedCardRequests = [];
        removedCardRequests.AddRange(Suggestion.RemovedCards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
        );
        
        TCGPCardsRequest addedCardsRequests = new(addedCardRequests);
        TCGPCardsRequest removedCardsRequests = new(removedCardRequests);
        
        AddedCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(addedCardsRequests, loadThumbnail:true);
        RemovedCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(removedCardsRequests, loadThumbnail:true);
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

    private static int GetCardPrimaryTypeIndex(TCGPCard c)
    {
        string name = c.Type?.Name?.Trim() ?? string.Empty;
        return name.Equals("Pokemon", StringComparison.OrdinalIgnoreCase) ? 1
            : name.Equals("Fossil", StringComparison.OrdinalIgnoreCase) ? 2
            : name.Equals("Item", StringComparison.OrdinalIgnoreCase) ? 3
            : name.Equals("Tool", StringComparison.OrdinalIgnoreCase) ? 4
            : name.Equals("Supporter", StringComparison.OrdinalIgnoreCase) ? 5
            : 6;
    }

    #endregion
}