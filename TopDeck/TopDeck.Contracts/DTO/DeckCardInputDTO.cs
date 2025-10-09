namespace TopDeck.Contracts.DTO;

public record DeckCardInputDTO(
    string CollectionCode,
    int CollectionNumber,
    bool IsHighlighted
);
