using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class VerticalSpacerBase : ComponentBase
{
    #region Statements

    [Parameter] public double Value { get; set; }

    #endregion
}