using BFlux;

namespace Taores.Shared.UIStore;

public record VariantSelectionState : ImmutableState
{
    #region Statements
    
    /// <summary>
    /// Stores the selected quantities for each variant option for each product.
    /// Each entry represents a product and its associated variant selections.
    /// </summary>
    /// <code>
    /// IReadOnlyDictionary&lt;productId (int), IReadOnlyDictionary&lt;variantOptionId (int), quantity (int)&gt;&gt;
    /// </code>
    /// <remarks>
    /// <c>productId (int):</c> The unique identifier of the product.<br/>
    /// <c>variantOptionId (int):</c> The unique identifier of the variant option.<br/>
    /// <c>quantity (int):</c> The number of selected units for the given variant option.<br/>
    /// </remarks>
    public IReadOnlyDictionary<int, IReadOnlyDictionary<int, int>> Values { get; init; }

    public VariantSelectionState(IReadOnlyDictionary<int, IReadOnlyDictionary<int, int>> values)
    {
        Values = values;
    }

    #endregion
}