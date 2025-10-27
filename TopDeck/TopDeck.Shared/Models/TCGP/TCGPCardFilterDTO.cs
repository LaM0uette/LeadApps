namespace TopDeck.Shared.Models.TCGP;

public record TCGPCardFilterDTO(
    string? Search,
    IReadOnlyList<string>? TypeNames,
    IReadOnlyList<string>? CollectionCodes,
    string? OrderBy,
    bool Asc
);
