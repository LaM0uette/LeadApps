namespace TopDeck.Shared.Models.TCGP;

public record TCGPCardRef(
    string Name,
    int TypeId,
    string CollectionCode,
    int CollectionNumber,
    string ImageUrl
);