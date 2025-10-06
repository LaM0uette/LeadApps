using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckLikeOutputDTO(
    [property: JsonPropertyName("deck")] DeckOutputDTO Deck,
    [property: JsonPropertyName("user")] UserOutputDTO User
);
