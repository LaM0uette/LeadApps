using BFlux;

namespace Taores.Shared.UIStore;

public record ClearVariantSelectionAction : ImmutableAction<VariantSelectionState>
{
    #region ImmutableAction

    public override VariantSelectionState Reduce(VariantSelectionState state)
    {
        return new VariantSelectionState(new Dictionary<int, IReadOnlyDictionary<int, int>>());
    }

    #endregion
}