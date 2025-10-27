namespace TopDeck.Contracts.DTO;

public record CardOutputDTO(
    CardTypeOutputDTO Type,
    string Name,
    string? ImageUrl,
    CardCollectionOutputDTO Collection,
    int CollectionNumber
);
