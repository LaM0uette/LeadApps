namespace TopDeck.Domain.Models;

public record DeckSuggestion(
    int Id,
    User Suggestor,
    Deck Deck,
    ICollection<CardRef> AddedCards,
    ICollection<CardRef> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds,
    ICollection<DeckSuggestionLike> Likes,
    ICollection<DeckSuggestionDislike> Dislikes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);