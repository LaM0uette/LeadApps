using Localizer;
using Microsoft.AspNetCore.Components;

namespace TopDeck.Client.Layout;

public class MainLayoutBase : LayoutComponentBase
{
    #region Statements

    [Inject] protected ILocalizer Localizer { get; set; } = null!;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Localizer.InitializeAsync();
            StateHasChanged();
        }
    }
    
    #endregion
}