using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DecksViewBase : LocalizedComponentBase, IAsyncDisposable
{
    #region Statements
    
    protected List<Deck> Decks { get; } = [];
    protected bool IsLoading { get; private set; }
    protected bool HasMore { get; private set; } = true;

    private int _skip;
    private const int _take = 20;
    private bool _jsReady;
    private bool _prefillInProgress;
    private long _lastLoadTicks;
    private bool _pendingBottomTrigger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    
    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;
    
    private DotNetObjectReference<DecksViewBase>? _objRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _js is IJSInProcessRuntime && !_jsReady)
        {
            // Prevent JS-triggered callbacks from kicking off loads before we prefill
            _prefillInProgress = true;
            
            _objRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("TopDeck.registerInfiniteScroll", _objRef, "#deck-scroll", 800);
            
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
            bool canScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
            
            if (!canScroll)
            {
                const int targetCount = _take * 2;
                int iterations = 0;
                const int maxIterations = 5;
                
                while (Decks.Count < targetCount && HasMore && iterations < maxIterations)
                {
                    await LoadMoreAsync();
                    await InvokeAsync(StateHasChanged);
                    iterations++;
                    
                    bool afterCanScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
                    
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
            
            IReadOnlyList<Deck> page = await _deckService.GetPageAsync(_skip, _take);
            
            if (page.Count > 0)
            {
                Decks.AddRange(page);
                
                _skip += page.Count;
                
                if (page.Count < _take)
                {
                    HasMore = false;
                    
                    if (_jsReady)
                    {
                        await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                    }
                }
            }
            else
            {
                HasMore = false;
                
                if (_jsReady)
                {
                    await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
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

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_jsReady)
            {
                await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll"); 
                
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