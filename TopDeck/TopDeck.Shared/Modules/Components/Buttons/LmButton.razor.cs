using LocalizedComponent;
using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class LmButtonBase : LocalizedComponentBase
{
    #region Statements

    [Parameter] public string style { get; set; } = string.Empty;
    
    [Parameter] public string Width { get; set; } = "auto";
    [Parameter] public string Height { get; set; } = "36px";
    [Parameter] public EventCallback Clicked { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public bool ForceLoad { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    #endregion

    #region Methods

    protected void OnClicked()
    {
        Clicked.InvokeAsync();
        
        if (!string.IsNullOrEmpty(Href))
        {
            _navigationManager.NavigateTo(Href, ForceLoad);
        }
    }

    #endregion
}