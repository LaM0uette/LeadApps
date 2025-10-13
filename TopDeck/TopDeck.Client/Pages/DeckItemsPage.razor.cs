using Microsoft.AspNetCore.Components;
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
    protected bool HasMore { get; private set; } = true;

    private int _skip;
    private const int _take = 30;
    private bool _jsReady;
    private bool _prefillInProgress;
    private long _lastLoadTicks;
    private bool _pendingBottomTrigger;
    private int _activeLoads; // number of in-flight background loads
    
    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    
    private DotNetObjectReference<DeckItemsPagePresenter>? _objRef;
    
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
        if (!HasMore)
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
        if (!HasMore) return;
        
        // Capture the skip value for this request and immediately allocate placeholders
        int requestSkip = _skip;
        int count = _take;
        int startIndex = DeckItems.Count;
        
        // Add placeholders to avoid blocking scroll
        for (int i = 0; i < count; i++)
        {
            DeckItems.Add(new DeckItem(
                Id: 0,
                CreatorUui: string.Empty,
                Name: string.Empty,
                Code: string.Empty,
                HighlightedCards: new List<DeckItemCard>(),
                EnergyIds: new List<int>(),
                TagIds: new List<int>(),
                LikeUserUuids: new List<string>(),
                DislikeUserUuids: new List<string>(),
                CreatedAt: DateTime.UtcNow
            ));
        }
        _skip += count; // reserve the range for subsequent loads
        
        _activeLoads++;
        IsLoading = _activeLoads > 0;
        await InvokeAsync(StateHasChanged);
        
        // Fire-and-forget the actual data load; fill placeholders when ready
        _ = Task.Run(async () =>
        {
            try
            {
                IReadOnlyList<DeckItem> page = await _deckItemService.GetPageAsync(requestSkip, count);
                //await Task.Delay(2000); // debug slow network simulation
                
                await InvokeAsync(async () =>
                {
                    int received = page.Count;
                    if (received > 0)
                    {
                        // Replace placeholders with real items
                        for (int i = 0; i < received; i++)
                        {
                            int idx = startIndex + i;
                            if (idx < DeckItems.Count)
                                DeckItems[idx] = page[i];
                            else
                                DeckItems.Add(page[i]);
                        }
                        
                        if (received < count)
                        {
                            // Remove extra placeholders and mark end
                            int extra = count - received;
                            int removeAt = Math.Min(startIndex + received, DeckItems.Count);
                            int canRemove = Math.Min(extra, DeckItems.Count - removeAt);
                            if (canRemove > 0)
                            {
                                DeckItems.RemoveRange(removeAt, canRemove);
                            }
                            HasMore = false;
                            if (_jsReady)
                            {
                                await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                            }
                        }
                    }
                    else
                    {
                        // No results: remove all placeholders and stop
                        int removeAt = Math.Min(startIndex, DeckItems.Count);
                        int canRemove = Math.Min(count, DeckItems.Count - removeAt);
                        if (canRemove > 0)
                            DeckItems.RemoveRange(removeAt, canRemove);
                        
                        HasMore = false;
                        if (_jsReady)
                        {
                            await JS.InvokeVoidAsync("TopDeck.unregisterInfiniteScroll");
                        }
                    }
                    
                    StateHasChanged();
                });
            }
            finally
            {
                _lastLoadTicks = DateTime.UtcNow.Ticks;
                _activeLoads--;
                if (_activeLoads < 0) _activeLoads = 0;
                IsLoading = _activeLoads > 0;
                await InvokeAsync(StateHasChanged);
            }
        });
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