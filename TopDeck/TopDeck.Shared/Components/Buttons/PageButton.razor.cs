using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class PageButtonBase : PresenterBase
{
    #region Statements

    [Parameter] public int Page { get; set; }
    [Parameter] public bool IsCurrent { get; set; }
    [Parameter] public bool IsEllipsis { get; set; }
    [Parameter] public EventCallback<int> Clicked { get; set; }

    #endregion
}