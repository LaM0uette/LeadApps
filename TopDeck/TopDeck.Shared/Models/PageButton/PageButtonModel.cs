namespace TopDeck.Shared.Models;

public readonly struct PageButtonModel
{
    #region Statements

    public int Page { get; }
    public bool IsCurrent { get; }
    public bool IsEllipsis { get; }
    
    public PageButtonModel(int page, bool isCurrent, bool isEllipsis = false)
    {
        Page = page;
        IsCurrent = isCurrent;
        IsEllipsis = isEllipsis;
    }

    #endregion

    #region Methods

    public static PageButtonModel Ellipsis()
    {
        return new PageButtonModel(-1, false, true);
    }

    #endregion
}