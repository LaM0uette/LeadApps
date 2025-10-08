namespace TopDeck.Contracts.AuthenticationStateSyncer;

public record OAuthUserInfo(
    string Id,
    string Name,
    string Email
);