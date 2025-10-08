using BFlux;

namespace TopDeck.Shared.UIStore.States.AuthenticatedUser;

public record SetCurrentAuthenticatedUserAction(string OAuthId) : ImmutableAction<AuthenticatedUserState>
{
    public override AuthenticatedUserState Reduce(AuthenticatedUserState state)
    {
        if (string.IsNullOrEmpty(OAuthId))
            throw new ArgumentException("OAuthId cannot be null or empty.", nameof(OAuthId));
        
        return new AuthenticatedUserState(OAuthId);
    }
}