using BFlux;

namespace TopDeck.Shared.UIStore.States.AuthenticatedUser;

public record AuthenticatedUserState(int Id, string? Uuid) : ImmutableState;