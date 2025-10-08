using BFlux;

namespace TopDeck.Shared.UIStore.States.AuthenticatedUser;

public record AuthenticatedUserState(string? OAuthId) : ImmutableState;