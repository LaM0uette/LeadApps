namespace TopDeck.Domain.Models;

public record DeckDislike(
    Deck Deck,
    User User
);
