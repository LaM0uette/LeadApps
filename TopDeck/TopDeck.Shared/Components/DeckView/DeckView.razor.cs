using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class DeckViewBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required Deck Deck { get; set; }

    #endregion

    #region Methods

    protected string GetEnergyClass(IEnumerable<int> energieIds)
    {
        int id = energieIds.FirstOrDefault();
        return id <= 0 ? "energy-none" : $"energy-{id}";
    }

    #endregion
}