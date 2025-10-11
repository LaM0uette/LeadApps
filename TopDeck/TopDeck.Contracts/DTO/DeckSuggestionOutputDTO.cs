using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckSuggestionOutputDTO(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("suggestor")] UserOutputDTO Suggestor,
    [property: JsonPropertyName("deck")] DeckOutputDTOold Deck,
    [property: JsonPropertyName("addedCards")] ICollection<DeckCardOutputDTOold> AddedCards,
    [property: JsonPropertyName("removedCards")] ICollection<DeckCardOutputDTOold> RemovedCards,
    [property: JsonPropertyName("addedEnergyIds")] ICollection<int> AddedEnergyIds,
    [property: JsonPropertyName("removedEnergyIds")] ICollection<int> RemovedEnergyIds,
    [property: JsonPropertyName("likes")] ICollection<DeckSuggestionLikeOutputDTO> Likes,
    [property: JsonPropertyName("dislikes")] ICollection<DeckSuggestionDislikeOutputDTO> Dislikes,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);