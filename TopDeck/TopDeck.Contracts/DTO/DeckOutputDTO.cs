using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckOutputDTO(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("creator")] UserOutputDTO Creator,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("cards")] ICollection<CardRefDTO> Cards,
    [property: JsonPropertyName("energyIds")] ICollection<int> EnergyIds,
    [property: JsonPropertyName("likes")] ICollection<DeckLikeOutputDTO> Likes,
    [property: JsonPropertyName("dislikes")] ICollection<DeckDislikeOutputDTO> Dislikes,
    [property: JsonPropertyName("suggestions")] ICollection<DeckSuggestionOutputDTO> Suggestions,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);