using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record CardRefDTO(
    [property: JsonPropertyName("collectionCode")] string CollectionCode,
    [property: JsonPropertyName("collectionNumber")] int CollectionNumber
);
