namespace TopDeck.Domain.Models;

public record DeckSuggestionLike(
    DeckSuggestion DeckSuggestion,
    User User
);
