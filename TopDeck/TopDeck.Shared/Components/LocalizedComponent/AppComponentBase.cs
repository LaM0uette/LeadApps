using Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Shared.Models;

namespace LocalizedComponent;

public class AppComponentBase : ComponentBase, IAsyncDisposable
{
    #region Statements

    [Inject] protected IJSRuntime JS { get; set; } = null!;
    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    
    protected bool IsMobile => _windowWidth < 768;

    private DotNetObjectReference<AppComponentBase>? _objRef;
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

    public async ValueTask DisposeAsync()
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