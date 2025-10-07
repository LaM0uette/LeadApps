namespace TopDeck.Domain.Models;

public record Deck(
    int Id,
    User Creator,
    string Name,
    string Code,
    ICollection<int> CardIds,
    ICollection<int> EnergyIds,
    ICollection<DeckLike> Likes,
    ICollection<DeckSuggestion> Suggestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);