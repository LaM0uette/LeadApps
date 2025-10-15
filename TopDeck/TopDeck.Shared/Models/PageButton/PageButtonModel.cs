namespace TopDeck.Shared.Models;

public readonly struct PageButtonModel
{
    public int Page { get; }
    public bool IsCurrent { get; }
    public bool IsEllipsis { get; }
    
    public PageButtonModel(int page, bool isCurrent)
    {
        Page = page;
        IsCurrent = isCurrent;
        IsEllipsis = false;
    }
    
    private PageButtonModel(bool _) 
    { 
        Page = -1; 
        IsCurrent = false; IsEllipsis = true; 
    }
    
    public static PageButtonModel Ellipsis()
    {
        return new PageButtonModel(true);
    }
}