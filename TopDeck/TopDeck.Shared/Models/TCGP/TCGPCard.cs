namespace TopDeck.Shared.Models.TCGP;

public record TCGPCard(
    int Id,
    TCGPCardType Type,
    string Name,
    string? Description,
    string? ImageUrl,
    List<TCGPCardSpecial> Specials,
    TCGPCardRarity Rarity,
    TCGPCardCollection Collection,
    int CollectionNumber
);