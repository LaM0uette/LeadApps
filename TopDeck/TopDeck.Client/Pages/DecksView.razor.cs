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
    private const int _take = 40;
    private bool _isLoading = false;
    private bool _hasMore = true;
    private bool _jsReady = false;

    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;

    private DotNetObjectReference<DecksViewBase>? _objRef;

    protected override async Task OnInitializedAsync()
    {
        await LoadMoreAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _js is IJSInProcessRuntime && !_jsReady)
        {
            _objRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("TopDeck.registerInfiniteScroll", _objRef, "#deck-scroll", 6000);
            _jsReady = true;
            await EnsureInitialScrollAsync();
        }
    }

    [JSInvokable]
    public async Task OnNearBottom()
    {
        if (_isLoading || !_hasMore) return;
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
            _isLoading = false;
        }
    }

    private async Task EnsureInitialScrollAsync()
    {
        if (!_jsReady || !_hasMore) return;
        try
        {
            bool canScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
            if (!canScroll)
            {
                int targetCount = _take * 2;
                while (Decks.Count < targetCount && _hasMore)
                {
                    await LoadMoreAsync();
                    await InvokeAsync(StateHasChanged);
                    bool afterCanScroll = await _js.InvokeAsync<bool>("TopDeck.canScroll", "#deck-scroll", 0);
                    if (afterCanScroll) break;
                }
            }
        }
        catch { }
    }

    public async ValueTask DisposeAsync()
    {
        try { if (_jsReady) { await _js.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll"); } } catch { }
        _objRef?.Dispose();
    }

    #endregion
}