using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckDislikeOutputDTO(
    [property: JsonPropertyName("deck")] DeckOutputDTOold Deck,
    [property: JsonPropertyName("user")] UserOutputDTO User
);
