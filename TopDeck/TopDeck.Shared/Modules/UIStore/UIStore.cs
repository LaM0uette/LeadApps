using BFlux;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Shared.UIStore;

public class UIStore : Store
{
    public UIStore()
    {
        States.Add(new AuthenticatedUserState(-1, string.Empty));
    }
}