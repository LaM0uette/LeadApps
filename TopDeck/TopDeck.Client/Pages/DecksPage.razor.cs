using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DecksPagePresenter : PresenterBase
{
    #region Statements
    
    protected List<DeckItem> DeckItems { get; } = [];
    protected bool IsLoading { get; private set; }
    protected bool HasMore { get; private set; } = true;

    private int _skip;
    private const int _take = 30;
    private bool _jsReady;
    private bool _prefillInProgress;
    private long _lastLoadTicks;
    private bool _pendingBottomTrigger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    
    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    
    private DotNetObjectReference<DecksPagePresenter>? _objRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && JS is IJSInProcessRuntime && !_jsReady)
        {
            // Prevent JS-triggered callbacks from kicking off loads before we prefill
            _prefillInProgress = true;
            
            _objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("TopDeck.registerInfiniteScroll", _objRef, "#deck-scroll", 800);
            
            _jsReady = true;
            await EnsureInitialScrollAsync();
        }
    }

    #endregion

    #region Methods

    [JSInvokable]
    public async Task OnNearBottom()
    {
        if (_prefillInProgress)
        {
            _pendingBottomTrigger = true;
            return;
        }
        if (IsLoading || !HasMore)
            return;
        
        long now = DateTime.UtcNow.Ticks;
        
        if (now - _lastLoadTicks < TimeSpan.FromMilliseconds(250).Ticks) 
            return;
        
        await LoadMoreAsync();
        StateHasChanged();
    }
    

    private async Task EnsureInitialScrollAsync()
    {
        if (!_jsReady || !HasMore) 
            return;
        
        _prefillInProgress = true;
        
        try
        {
            bool canScroll = await JS.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
            
            if (!canScroll)
            {
                const int targetCount = _take * 2;
                int iterations = 0;
                const int maxIterations = 5;
                
                while (DeckItems.Count < targetCount && HasMore && iterations < maxIterations)
                {
                    await LoadMoreAsync();
                    await InvokeAsync(StateHasChanged);
                    iterations++;
                    
                    bool afterCanScroll = await JS.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
                    
                    if (afterCanScroll) 
                        break;
                }
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _prefillInProgress = false;
            if (_pendingBottomTrigger && HasMore && !IsLoading)
            {
                _pendingBottomTrigger = false;
                await LoadMoreAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
    }
    
    private async Task LoadMoreAsync()
    {
        await _loadLock.WaitAsync();
        try
        {
            IsLoading = true;
            await InvokeAsync(StateHasChanged);
            
            // Ensure the UI has a chance to render the loader before starting the network call
            await Task.Yield();
            
            IReadOnlyList<DeckItem> page = await _deckItemService.GetPageAsync(_skip, _take);
            
            if (page.Count > 0)
            {
                DeckItems.AddRange(page);
                
                _skip += page.Count;
                
                if (page.Count < _take)
                {
                    HasMore = false;
                    
                    if (_jsReady)
                    {
                        await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                    }
                }
            }
            else
            {
                HasMore = false;
                
                if (_jsReady)
                {
                    await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                }
            }
        }
        finally
        {
            _lastLoadTicks = DateTime.UtcNow.Ticks;
            IsLoading = false;
            _loadLock.Release();
            
            // Ensure the loader disappears promptly after the load completes
            await InvokeAsync(StateHasChanged);
        }
    }
    
    #endregion

    #region IAsyncDisposable

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        
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