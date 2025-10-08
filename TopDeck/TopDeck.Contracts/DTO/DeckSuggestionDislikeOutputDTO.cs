using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record DeckSuggestionDislikeOutputDTO(
    [property: JsonPropertyName("deckSuggestion")] DeckSuggestionOutputDTO DeckSuggestion,
    [property: JsonPropertyName("user")] UserOutputDTO User
);
