namespace TopDeck.Domain.Models;

public record DeckSuggestion(
    int Id,
    User Suggestor,
    Deck Deck,
    ICollection<int> AddedCardIds,
    ICollection<int> RemovedCardIds,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds,
    ICollection<DeckSuggestionLike> Likes,
    ICollection<DeckSuggestionDislike> Dislikes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);