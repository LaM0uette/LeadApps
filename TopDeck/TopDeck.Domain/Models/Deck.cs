namespace TopDeck.Domain.Models;

public record Deck(
    int Id,
    User Creator,
    string Name,
    string Code,
    ICollection<CardRef> Cards,
    ICollection<CardRef> HighlightedCards,
    ICollection<int> EnergyIds,
    ICollection<DeckLike> Likes,
    ICollection<DeckDislike> Dislikes,
    ICollection<DeckSuggestion> Suggestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);