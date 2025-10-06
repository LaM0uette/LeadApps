using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckSuggestionOutputDTO(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("suggestor")] UserOutputDTO Suggestor,
    [property: JsonPropertyName("deck")] DeckOutputDTO Deck,
    [property: JsonPropertyName("addedCardIds")] ICollection<int> AddedCardIds,
    [property: JsonPropertyName("removedCardIds")] ICollection<int> RemovedCardIds,
    [property: JsonPropertyName("addedEnergyIds")] ICollection<int> AddedEnergyIds,
    [property: JsonPropertyName("removedEnergyIds")] ICollection<int> RemovedEnergyIds,
    [property: JsonPropertyName("likes")] ICollection<DeckSuggestionLikeOutputDTO> Likes,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);