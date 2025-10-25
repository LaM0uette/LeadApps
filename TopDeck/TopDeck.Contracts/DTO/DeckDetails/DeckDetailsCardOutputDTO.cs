namespace TopDeck.Contracts.DTO;

public record DeckDetailsCardOutputDTO(
    string CollectionCode,
    int CollectionNumber,
    bool IsHighlighted
);
