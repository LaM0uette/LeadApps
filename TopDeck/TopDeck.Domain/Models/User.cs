namespace TopDeck.Domain.Models;

public record User(
    int Id,
    string OAuthProvider,
    string OAuthId,
    string UserName,
    string Email,
    DateTime CreatedAt
);