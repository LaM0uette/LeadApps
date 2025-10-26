namespace TopDeck.Shared.Models.TCGP;

public record TCGPPokemonCard(
    TCGPCardType Type,
    string Name,
    string? ImageUrl,
    TCGPCardCollection Collection,
    int CollectionNumber,
    List<TCGPPokemonSpecial> PokemonSpecials,
    TCGPPokemonType PokemonType
) : TCGPCard(
    Type,
    Name,
    ImageUrl,
    Collection,
    CollectionNumber
);
