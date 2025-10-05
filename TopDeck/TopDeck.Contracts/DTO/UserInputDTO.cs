namespace TopDeck.Contracts.DTO;

public record UserInputDTO(
    string OAuthProvider,
    string OAuthId,
    string UserName,
    string Email
);