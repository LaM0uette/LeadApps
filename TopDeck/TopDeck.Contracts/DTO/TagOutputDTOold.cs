using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record TagOutputDTOold(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("colorHex")] string ColorHex
);
