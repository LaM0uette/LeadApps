namespace TopDeck.Contracts.DTO;

public record UserOutputDTO(
    int Id,
    string OAuthProvider,
    string Uuid,
    string UserName,
    DateTime CreatedAt
);