namespace TopDeck.Domain.Models;

public record DeckSuggestionDislike(
    DeckSuggestion DeckSuggestion,
    User User
);
