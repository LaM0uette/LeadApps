namespace TopDeck.Contracts.DTO;

public record DeckItemCardInputDTO(
    string CollectionCode,
    int CollectionNumber,
    bool IsHighlighted
);
