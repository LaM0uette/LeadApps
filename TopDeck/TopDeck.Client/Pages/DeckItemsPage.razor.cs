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
        if (Page <= 1) return;
        NavigateToPage(Page - 1);
    }

    protected void NextPage()
    {
        if (!HasNextPage) return;
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
            HasNextPage = items.Count == Size;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void NavigateToPage(int page)
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