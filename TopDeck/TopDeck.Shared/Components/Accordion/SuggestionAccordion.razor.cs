using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class SuggestionAccordionBase : LocalizedComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required DeckSuggestion Suggestion { get; set; }
    [Parameter, EditorRequired] public required string Width { get; set; } = "100%";
    [Parameter, EditorRequired] public required string Height { get; set; } = "45px";
    [Parameter, EditorRequired] public required string FontSize { get; set; } = "1em";

    protected bool IsOpen;
    
    protected readonly Dictionary<int, string> EnergyTypes = new()
    {
        { 1, "Grass" },
        { 2, "Fire" },
        { 3, "Water" },
        { 4, "Lightning" },
        { 5, "Psychic" },
        { 6, "Fighting" },
        { 7, "Darkness" },
        { 8, "Metal" },
        { 9, "Dragon" },
        { 10, "Colorless" }
    };

    #endregion

    #region Methods

    protected void Toggle()
    {
        IsOpen = !IsOpen;
    }

    #endregion
}