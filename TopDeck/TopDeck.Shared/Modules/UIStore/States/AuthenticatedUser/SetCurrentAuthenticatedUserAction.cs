using BFlux;

namespace TopDeck.Shared.UIStore.States.AuthenticatedUser;

public record SetCurrentAuthenticatedUserAction(int Id, string Uuid) : ImmutableAction<AuthenticatedUserState>
{
    public override AuthenticatedUserState Reduce(AuthenticatedUserState state)
    {
        if (string.IsNullOrEmpty(Uuid))
            throw new ArgumentException("Uuid cannot be null or empty.", nameof(Uuid));
        
        return new AuthenticatedUserState(Id, Uuid);
    }
}