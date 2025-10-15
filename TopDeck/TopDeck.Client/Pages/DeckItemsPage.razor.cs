using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DeckItemsPagePresenter : PresenterBase
{
    #region Statements
    
    private const int MAX_PAGE_SIZE = 100;
    private const int DEFAULT_PAGE_SIZE = 40;
    
    [SupplyParameterFromQuery] public int Page { get; set; } = 1;
    [SupplyParameterFromQuery] public int Size { get; set; } = DEFAULT_PAGE_SIZE;
     
    protected List<DeckItem> DeckItems { get; } = [];
    protected bool HasNextPage { get; private set; }
    protected bool IsLoading { get; private set; }

    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private NavigationManager _nav { get; set; } = null!;

    private DotNetObjectReference<DeckItemsPagePresenter>? _presenterRef;
    private bool _restoreScrollPending;
    private string _scrollKey => $"decks:p{Page}:s{Size}";

    private int _deckItemCount;
    private int _maxPage;
    
    protected override async Task OnParametersSetAsync()
    {
        if (Page <= 0)
        {
            Page = 1;
        }
        
        if (Size is <= 0 or > MAX_PAGE_SIZE)
        {
            Size = DEFAULT_PAGE_SIZE;
        }

        _deckItemCount = await _deckItemService.GetDeckItemCountAsync();
        _maxPage = Math.Max(1, (int)Math.Ceiling(_deckItemCount / (double)Size));
        
        if (Page > _maxPage)
        {
            Page = _maxPage;
            NavigateToPage(Page);
            return;
        }

        await LoadPageAsync();
        _restoreScrollPending = true;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _presenterRef = DotNetObjectReference.Create(this);
        }
        
        if (_restoreScrollPending && JS is IJSInProcessRuntime)
        {
            try
            {
                await JS.InvokeVoidAsync("TopDeck.restoreScroll", _scrollKey, "#deck-scroll");
            }
            catch
            {
                // ignored
            }
            
            _restoreScrollPending = false;
        }
    }

    #endregion

    #region Methods
    
    protected void PrevPage()
    {
        if (IsLoading || Page <= 1) 
            return;
        
        NavigateToPage(Page - 1);
    }

    protected void NextPage()
    {
        if (IsLoading || Page >= _maxPage || !HasNextPage)
            return;
        
        NavigateToPage(Page + 1);
    }
    

    private async Task LoadPageAsync()
    {
        IsLoading = true;
        DeckItems.Clear();
        StateHasChanged();
        
        try
        {
            int skip = (Page - 1) * Size;
            
            if (skip < 0) 
                skip = 0;
            
            IReadOnlyList<DeckItem> items = await _deckItemService.GetPageAsync(skip, Size);
            DeckItems.AddRange(items);
            HasNextPage = Page < _maxPage;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void NavigateToPage(int page)
    {
        Uri uri = new(_nav.Uri);
        string basePath = uri.GetLeftPart(UriPartial.Path);
        string target = $"{basePath}?page={page}&size={Size}";
        
        _nav.NavigateTo(target);
    }

    private async Task SaveScroll()
    {
        if (JS is not IJSInProcessRuntime) 
            return;
        
        try
        {
            await JS.InvokeVoidAsync("TopDeck.saveScroll", _scrollKey, "#deck-scroll");
        }
        catch
        {
            // ignored
        }
    }

    protected readonly struct PaginationButton
    {
        public int Page { get; }
        public bool IsCurrent { get; }
        public bool IsEllipsis { get; }
        public PaginationButton(int page, bool isCurrent)
        {
            Page = page;
            IsCurrent = isCurrent;
            IsEllipsis = false;
        }
        private PaginationButton(bool _) { Page = -1; IsCurrent = false; IsEllipsis = true; }
        public static PaginationButton Ellipsis() => new(true);
    }

    protected IEnumerable<PaginationButton> GetPageButtons()
    {
        // Desired pattern example for 13 pages on page 6: 1 … 4 5 6 7 8 … 13
        // Rules:
        // - Always show first and last pages.
        // - Show a window of pages around current: [current-2, current+2].
        // - Insert ellipses when there's a gap greater than 1 between consecutive shown pages.
        int last = _maxPage;
        if (last <= 1)
        {
            yield return new PaginationButton(1, Page == 1);
            yield break;
        }

        int current = Math.Clamp(Page, 1, last);
        int windowStart = Math.Max(1, current - 2);
        int windowEnd = Math.Min(last, current + 2);

        // Always first
        yield return new PaginationButton(1, current == 1);

        // Ellipsis after first if needed
        if (windowStart > 2)
        {
            yield return PaginationButton.Ellipsis();
        }

        // Middle window (avoid duplicating first/last)
        int middleStart = Math.Max(2, windowStart);
        int middleEnd = Math.Min(last - 1, windowEnd);
        for (int p = middleStart; p <= middleEnd; p++)
        {
            yield return new PaginationButton(p, p == current);
        }

        // Ellipsis before last if needed
        if (windowEnd < last - 1)
        {
            yield return PaginationButton.Ellipsis();
        }

        // Always last
        yield return new PaginationButton(last, current == last);
    }
    
    #endregion

    #region IAsyncDisposable

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await SaveScroll();
        }
        catch
        {
            // ignored
        }

        await base.DisposeAsync();
        
        try
        {
            await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll"); 
        }
        catch
        {
            // ignored
        }

        _presenterRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}