using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models;
using TopDeck.Shared.Services;
using DomainTag = TopDeck.Domain.Models.Tag;

namespace TopDeck.Client.Pages;

public class DeckItemsPagePresenter : PresenterBase
{
    #region Statements
    
    private const int MAX_PAGE_SIZE = 100;
    private const int DEFAULT_PAGE_SIZE = 40;
    
    [SupplyParameterFromQuery] public int Page { get; set; } = 1;
    [SupplyParameterFromQuery] public int Size { get; set; } = DEFAULT_PAGE_SIZE;

    // Filters bound to query string
    [SupplyParameterFromQuery] public string? Search { get; set; }
    [SupplyParameterFromQuery] public int[] TagIds { get; set; } = Array.Empty<int>();
    [SupplyParameterFromQuery] public string? OrderBy { get; set; }
    [SupplyParameterFromQuery] public bool Asc { get; set; }
     
    protected List<DeckItem> DeckItems { get; } = [];
    protected bool HasNextPage { get; private set; }
    protected bool IsLoading { get; private set; }

    [Inject] private IDeckItemService _deckItemService { get; set; } = null!;
    [Inject] private ITagService _tagService { get; set; } = null!;
    [Inject] private NavigationManager _nav { get; set; } = null!;

    private DotNetObjectReference<DeckItemsPagePresenter>? _presenterRef;
    private bool _restoreScrollPending;
    private string _scrollKey => $"decks:p{Page}:s{Size}:q{Search}:t{string.Join('-', TagIds)}:o{OrderBy}:a{Asc}";

    private int _deckItemCount;
    private int _maxPage;

    // Filter popup state
    protected bool IsFilterOpen { get; private set; }
    protected string? SearchInput { get; set; }
    protected string? OrderByInput { get; set; }
    protected bool AscInput { get; set; }
    protected List<DomainTag> AllTags { get; private set; } = [];
    protected HashSet<int> SelectedTagIds { get; private set; } = [];
    
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

        // Load tags once
        if (AllTags.Count == 0)
        {
            AllTags = (await _tagService.GetAllAsync())?.ToList() ?? [];
        }

        // Sync popup inputs from current query-bound filters
        SearchInput = Search;
        OrderByInput = string.IsNullOrWhiteSpace(OrderBy) ? "updatedAt" : OrderBy;
        AscInput = Asc;
        SelectedTagIds = TagIds.Length > 0 ? TagIds.ToHashSet() : [];

        _deckItemCount = await _deckItemService.GetDeckItemCountAsync(Search, SelectedTagIds.ToList(), default);
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
    
    protected void NavigateToPage(int page)
    {
        Uri uri = new(_nav.Uri);
        string basePath = uri.GetLeftPart(UriPartial.Path);
        var query = new List<string>
        {
            $"page={page}",
            $"size={Size}"
        };
        if (!string.IsNullOrWhiteSpace(Search)) query.Add($"search={Uri.EscapeDataString(Search)}");
        if (TagIds is { Length: > 0 })
        {
            foreach (int id in TagIds.Distinct()) query.Add($"tagIds={id}");
        }
        if (!string.IsNullOrWhiteSpace(OrderBy)) query.Add($"orderBy={OrderBy}");
        if (Asc) query.Add("asc=true");
        string target = $"{basePath}?{string.Join("&", query)}";
        
        _nav.NavigateTo(target);
    }
    
    protected IEnumerable<PageButtonModel> GetPageButtons()
    {
        int last = _maxPage;
        
        if (last <= 1)
        {
            yield return new PageButtonModel(1, Page == 1);
            yield break;
        }

        int current = Math.Clamp(Page, 1, last);
        int windowStart = Math.Max(1, current - 2);
        int windowEnd = Math.Min(last, current + 2);

        yield return new PageButtonModel(1, current == 1);

        if (windowStart > 2)
        {
            yield return PageButtonModel.Ellipsis();
        }

        int middleStart = Math.Max(2, windowStart);
        int middleEnd = Math.Min(last - 1, windowEnd);
        for (int p = middleStart; p <= middleEnd; p++)
        {
            yield return new PageButtonModel(p, p == current);
        }

        if (windowEnd < last - 1)
        {
            yield return PageButtonModel.Ellipsis();
        }

        yield return new PageButtonModel(last, current == last);
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
            
            IReadOnlyList<DeckItem> items = await _deckItemService.GetPageAsync(
                skip,
                Size,
                Search,
                TagIds,
                string.IsNullOrWhiteSpace(OrderBy) ? "updatedAt" : OrderBy,
                Asc);
            DeckItems.AddRange(items);
            HasNextPage = Page < _maxPage;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
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

    #region Filter UI

    protected void OpenFilter()
    {
        IsFilterOpen = true;
    }

    protected void CloseFilter()
    {
        IsFilterOpen = false;
    }

    protected void ToggleTag(int id)
    {
        if (SelectedTagIds.Contains(id)) SelectedTagIds.Remove(id); else SelectedTagIds.Add(id);
    }

    protected void ResetFilter()
    {
        SearchInput = null;
        OrderByInput = "updatedAt";
        AscInput = false;
        SelectedTagIds.Clear();
    }

    protected void ApplyFilter()
    {
        // Update query-bound properties and navigate to page 1
        Search = string.IsNullOrWhiteSpace(SearchInput) ? null : SearchInput;
        OrderBy = string.IsNullOrWhiteSpace(OrderByInput) ? null : OrderByInput;
        Asc = AscInput;
        TagIds = SelectedTagIds.Count > 0 ? SelectedTagIds.ToArray() : Array.Empty<int>();
        IsFilterOpen = false;
        NavigateToPage(1);
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