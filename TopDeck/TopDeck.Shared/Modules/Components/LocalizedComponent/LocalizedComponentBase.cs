using Localizer;
using Microsoft.AspNetCore.Components;

namespace LocalizedComponent;

public class LocalizedComponentBase : ComponentBase
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