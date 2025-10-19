namespace TopDeck.Shared.Models.TCGP;

public record TCGPCardRef(
    string Name,
    string CollectionCode,
    int CollectionNumber,
    string ImageUrl
);