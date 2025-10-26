namespace TopDeck.Contracts.DTO;

public record CardPokemonOutputDTO(
    CardTypeOutputDTO Type,
    string Name,
    string? ImageUrl,
    CardCollectionOutputDTO Collection,
    int CollectionNumber,
    List<PokemonSpecialOutputDTO> PokemonSpecials,
    PokemonTypeOutputDTO PokemonType
) : CardOutputDTO(
    Type,
    Name,
    ImageUrl,
    Collection,
    CollectionNumber
);
