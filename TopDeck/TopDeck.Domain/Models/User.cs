namespace TopDeck.Domain.Models;

public record User(
    int Id,
    string OAuthProvider,
    string Uuid,
    string UserName,
    DateTime CreatedAt
);