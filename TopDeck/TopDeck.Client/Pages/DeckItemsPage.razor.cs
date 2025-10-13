using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DeckItemsPagePresenter : PresenterBase
{
    #region Statements
    
    protected List<DeckItem> DeckItems { get; } = [];
    protected bool IsLoading { get; private set; }

    // Pagination state
    protected int CurrentPage { get; private set; } = 1; // 1-based
    protected int PageSize { get; private set; } = 30;
    protected bool HasNextPage { get; private set; }

    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private NavigationManager _nav { get; set; } = null!;

    private bool _restoreScrollPending;
    private bool _suppressSaveOnNavigation;
    private string ScrollKey => "decks";

    private DotNetObjectReference<DeckItemsPagePresenter>? _objRef;

    // Legacy fields kept to avoid breaking references while migrating away from infinite scroll
    private bool _jsReady;
    private bool _prefillInProgress;
    private long _lastLoadTicks;
    private bool _pendingBottomTrigger;
    private int _activeLoads;
    private int _skip;
    private const int _take = 30;
    
    protected override async Task OnParametersSetAsync()
    {
        await LoadFromUriAsync(_nav.Uri);
        _restoreScrollPending = true;
    }
    
    protected override Task OnInitializedAsync()
    {
        _nav.LocationChanged += OnLocationChanged;
        return base.OnInitializedAsync();
    }
    
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = InvokeAsync(async () =>
        {
            await LoadFromUriAsync(e.Location);
            _restoreScrollPending = true;
            StateHasChanged();
        });
    }
    
    private Task LoadFromUriAsync(string location)
    {
        Uri uri = new(location);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (int.TryParse(query.Get("page"), out int p) && p > 0) CurrentPage = p; else CurrentPage = 1;
        if (int.TryParse(query.Get("size"), out int s) && s > 0 && s <= 100) PageSize = s; else PageSize = 30;
        return LoadPageAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
        }
        if (_restoreScrollPending && JS is IJSInProcessRuntime)
        {
            try{ await JS.InvokeVoidAsync("TopDeck.restoreScroll", ScrollKey, "#deck-scroll"); } catch {}
            _restoreScrollPending = false;
        }
    }

    #endregion

    #region Methods

    [JSInvokable]
    public Task OnNearBottom()
    {
        // Infinite scroll disabled in favor of pagination
        return Task.CompletedTask;
    }
    

    private Task EnsureInitialScrollAsync()
    {
        // Infinite scroll removed; nothing to ensure
        return Task.CompletedTask;
    }
    
    private Task LoadMoreAsync()
    {
        // Infinite scroll removed
        return Task.CompletedTask;
    }

    // Pagination helpers
    private async Task LoadPageAsync()
    {
        IsLoading = true;
        DeckItems.Clear();
        StateHasChanged();
        try
        {
            int skip = (CurrentPage - 1) * PageSize;
            if (skip < 0) skip = 0;
            IReadOnlyList<DeckItem> items = await _deckItemService.GetPageAsync(skip, PageSize);
            DeckItems.AddRange(items);
            HasNextPage = items.Count == PageSize;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void PrevPage()
    {
        if (CurrentPage <= 1) return;
        _suppressSaveOnNavigation = true;
        ClearScroll();
        NavigateToPage(CurrentPage - 1);
    }

    protected void NextPage()
    {
        if (!HasNextPage) return;
        _suppressSaveOnNavigation = true;
        ClearScroll();
        NavigateToPage(CurrentPage + 1);
    }

    protected void GoToPage(int page)
    {
        if (page < 1) page = 1;
        _suppressSaveOnNavigation = true;
        ClearScroll();
        NavigateToPage(page);
    }

    private void NavigateToPage(int page)
    {
        var uri = new Uri(_nav.Uri);
        var basePath = uri.GetLeftPart(UriPartial.Path);
        var target = $"{basePath}?page={page}&size={PageSize}";
        _nav.NavigateTo(target);
    }

    private void SaveScroll()
    {
        if (JS is IJSInProcessRuntime)
        {
            try { JS.InvokeVoidAsync("TopDeck.saveScroll", ScrollKey, "#deck-scroll"); } catch {}
        }
    }

    private void ClearScroll()
    {
        if (JS is IJSInProcessRuntime)
        {
            try { JS.InvokeVoidAsync("TopDeck.clearScroll", ScrollKey); } catch {}
        }
    }
    
    #endregion

    #region IAsyncDisposable

    public override async ValueTask DisposeAsync()
    {
        // Save scroll position if we navigate away (but not during page change where we reset)
        try
        {
            if (!_suppressSaveOnNavigation)
            {
                SaveScroll();
            }
        }
        catch { }

        await base.DisposeAsync();
        
        try
        {
            _nav.LocationChanged -= OnLocationChanged;
        }
        catch { }
        
        try
        {
            if (_jsReady)
            {
                await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll"); 
                
            }
        }
        catch
        {
            // ignored
        }

        _objRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}