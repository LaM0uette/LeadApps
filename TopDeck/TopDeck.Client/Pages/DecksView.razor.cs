using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class DecksViewBase : LocalizedComponentBase, IAsyncDisposable
{
    #region Statements
    
    protected List<Deck> Decks { get; set; } = new();

    private int _skip = 0;
    private const int _take = 20;
    private bool _isLoading = false;
    private bool _hasMore = true;
    private bool _jsReady = false;
    private bool _prefillInProgress = false;
    private long _lastLoadTicks = 0;

    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;

    private DotNetObjectReference<DecksViewBase>? _objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _js is IJSInProcessRuntime && !_jsReady)
        {
            _objRef = DotNetObjectReference.Create(this);
            // Lower threshold to avoid triggering too early
            await _js.InvokeVoidAsync("TopDeck.registerInfiniteScroll", _objRef, "#deck-scroll", 800);
            _jsReady = true;
            await EnsureInitialScrollAsync();
        }
    }

    [JSInvokable]
    public async Task OnNearBottom()
    {
        if (_prefillInProgress || _isLoading || !_hasMore) return;
        // Extra guard against rapid successive triggers
        long now = DateTime.UtcNow.Ticks;
        if (now - _lastLoadTicks < TimeSpan.FromMilliseconds(250).Ticks) return;
        await LoadMoreAsync();
        StateHasChanged();
    }

    private async Task LoadMoreAsync()
    {
        _isLoading = true;
        try
        {
            IReadOnlyList<Deck> page = await _deckService.GetPageAsync(_skip, _take);
            if (page.Count > 0)
            {
                Decks.AddRange(page);
                
                _skip += page.Count;
                if (page.Count < _take)
                {
                    _hasMore = false;
                    if (_jsReady)
                    {
                        await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                    }
                }
            }
            else
            {
                _hasMore = false;
                if (_jsReady)
                {
                    await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                }
            }
        }
        finally
        {
            _lastLoadTicks = DateTime.UtcNow.Ticks;
            _isLoading = false;
        }
    }

    private async Task EnsureInitialScrollAsync()
    {
        if (!_jsReady || !_hasMore) return;
        _prefillInProgress = true;
        try
        {
            bool canScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
            if (!canScroll)
            {
                int targetCount = _take * 2;
                int iterations = 0;
                const int maxIterations = 5; // safety cap
                while (Decks.Count < targetCount && _hasMore && iterations < maxIterations)
                {
                    await LoadMoreAsync();
                    await InvokeAsync(StateHasChanged);
                    iterations++;
                    bool afterCanScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
                    if (afterCanScroll) break;
                }
            }
        }
        catch { }
        finally
        {
            _prefillInProgress = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { if (_jsReady) { await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll"); } } catch { }
        _objRef?.Dispose();
    }

    #endregion
}