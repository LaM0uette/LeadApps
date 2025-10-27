namespace TopDeck.Contracts.DTO;

public record CardToolOutputDTO(
    CardTypeOutputDTO Type,
    string Name,
    string? ImageUrl,
    CardCollectionOutputDTO Collection,
    int CollectionNumber
) : CardOutputDTO(
    Type,
    Name,
    ImageUrl,
    Collection,
    CollectionNumber
);
