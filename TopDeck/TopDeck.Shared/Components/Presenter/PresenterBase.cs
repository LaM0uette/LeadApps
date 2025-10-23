using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Shared.Models;

namespace TopDeck.Shared.Components;

public class PresenterBase : ComponentBase, IAsyncDisposable
{
    #region Statements

    [Inject] protected IJSRuntime JS { get; set; } = null!;
    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected UIStore.UIStore UIStore { get; set; } = null!;
    
    protected bool IsMobile => _windowWidth < 768;

    private DotNetObjectReference<PresenterBase>? _objRef;
    private int _windowWidth { get; set; }
    private int _windowHeight { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (JS.GetType().Name != "UnsupportedJavaScriptRuntime")
            {
                _objRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("registerResizeHandler", _objRef);

                WindowSize size = await JS.InvokeAsync<WindowSize>("getWindowSize");
                _windowWidth = size.Width;
                _windowHeight = size.Height;
            
                StateHasChanged();
            }
        }
    }
    
    #endregion

    #region Methods

    [JSInvokable]
    public void OnResize(WindowSize size)
    {
        _windowWidth = size.Width;
        _windowHeight = size.Height;
        
        InvokeAsync(StateHasChanged);
    }

    #endregion

    #region IAsyncDisposable

    public virtual async ValueTask DisposeAsync()
    {
        if (_objRef != null)
        {
            try
            {
                if (JS.GetType().Name != "UnsupportedJavaScriptRuntime")
                {
                    await JS.InvokeVoidAsync("unregisterResizeHandler");
                }
            }
            catch
            {
                // Ignore
            }

            _objRef.Dispose();
            _objRef = null;
        }
        
        GC.SuppressFinalize(this);
    }

    #endregion
}