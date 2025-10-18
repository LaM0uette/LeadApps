using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TopDeck.Shared.Components;

public class TitlePanelBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Name { get; set; }
    [Parameter] public string Width { get; set; } = "100px";
    [Parameter] public string Height { get; set; } = "26px";
    [Parameter] public string FontSize { get; set; } = "0.63em";
    
    [Parameter] public EventCallback<string> NameChanged { get; set; }

    protected bool IsEditing { get; set; }
    protected string EditableName { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        EditableName = Name;
    }
    
    #endregion

    #region Methods

    protected void StartEditing()
    {
        IsEditing = true;
    }

    protected async Task StopEditing()
    {
        IsEditing = false;
        if (EditableName != Name)
        {
            Name = EditableName;
            await NameChanged.InvokeAsync(Name);
        }
    }

    protected async Task OnNameChanged(ChangeEventArgs e)
    {
        EditableName = e.Value?.ToString() ?? string.Empty;
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await StopEditing();
        }
        else if (e.Key == "Escape")
        {
            EditableName = Name;
            IsEditing = false;
        }
    }

    #endregion
}