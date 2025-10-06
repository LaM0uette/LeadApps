using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckOutputDTO(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("creator")] UserOutputDTO Creator,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("cardIds")] ICollection<int> CardIds,
    [property: JsonPropertyName("energyIds")] ICollection<int> EnergyIds,
    [property: JsonPropertyName("likes")] ICollection<DeckLikeOutputDTO> Likes,
    [property: JsonPropertyName("suggestions")] ICollection<DeckSuggestionOutputDTO> Suggestions,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);