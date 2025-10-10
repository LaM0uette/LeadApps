namespace TopDeck.Domain.Models;

public record Deck(
    int Id,
    User Creator,
    string Name,
    string Code,
    ICollection<DeckCard> Cards,
    ICollection<int> EnergyIds,
    ICollection<Tag> Tags,
    ICollection<DeckLike> Likes,
    ICollection<DeckDislike> Dislikes,
    ICollection<DeckSuggestion> Suggestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);