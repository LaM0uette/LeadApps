namespace TopDeck.Contracts.DTO;

public record CardSupporterOutputDTO(
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
