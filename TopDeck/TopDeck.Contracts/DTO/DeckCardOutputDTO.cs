using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckCardOutputDTO(
    [property: JsonPropertyName("collectionCode")] string CollectionCode,
    [property: JsonPropertyName("collectionNumber")] int CollectionNumber,
    [property: JsonPropertyName("isHighlighted")] bool IsHighlighted
);
