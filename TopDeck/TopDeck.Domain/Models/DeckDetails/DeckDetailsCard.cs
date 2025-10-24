namespace TopDeck.Domain.Models;

public record DeckDetailsCard(
    string CollectionCode,
    int CollectionNumber,
    bool IsHighlighted
);
