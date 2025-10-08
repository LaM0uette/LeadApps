using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class DeckViewBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required Deck Deck { get; set; }

    protected string DeckCode = string.Empty;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;

    protected override void OnParametersSet()
    {
        DeckCode = Deck.Code;
        Console.WriteLine(Deck.Likes.Count);
        Console.WriteLine(Deck.Dislikes.Count);
    }

    #endregion

    #region Methods

    protected string GetEnergyClass(IEnumerable<int> energieIds)
    {
        int id = energieIds.FirstOrDefault();
        return id <= 0 ? "energy-none" : $"energy-{id}";
    }
    
    protected async Task CopyCode()
    {
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", Deck.Code);
        DeckCode = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        DeckCode = Deck.Code;
        StateHasChanged();
    }

    #endregion
}