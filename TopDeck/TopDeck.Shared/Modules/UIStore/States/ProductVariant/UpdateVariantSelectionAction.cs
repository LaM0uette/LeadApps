using BFlux;

namespace Taores.Shared.UIStore;

public record UpdateVariantSelectionAction : ImmutableAction<VariantSelectionState>
{
    #region Statements

    private readonly int _productId;
    private readonly IReadOnlyDictionary<int, int> _variantOptionsSelected;

    public UpdateVariantSelectionAction(int productId, IReadOnlyDictionary<int, int> variantOptionsSelected)
    {
        _productId = productId;
        _variantOptionsSelected = variantOptionsSelected;
    }

    #endregion

    #region ImmutableAction

    public override VariantSelectionState Reduce(VariantSelectionState state)
    {
        var productVariantOptions = new Dictionary<int, IReadOnlyDictionary<int, int>>(state.Values);
        
        if (_variantOptionsSelected.Count == 0)
        {
            productVariantOptions.Remove(_productId);
        }
        else
        {
            productVariantOptions[_productId] = new Dictionary<int, int>(_variantOptionsSelected);
        }
        
        return new VariantSelectionState(productVariantOptions);
    }

    #endregion
}