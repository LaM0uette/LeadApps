namespace TopDeck.Domain.Models;

public record DeckCard(
    string CollectionCode,
    int CollectionNumber,
    bool IsHighlighted
);
