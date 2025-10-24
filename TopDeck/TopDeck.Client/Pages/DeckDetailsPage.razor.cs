using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TCGPCardRequester;
using TopDeck.Domain.Models;
using TopDeck.Shared.Components;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Client.Pages;

public class DeckDetailsPagePresenter : PresenterBase
{
    #region Enums

    protected enum Mode
    {
        View,
        Edit
    }
    
    protected enum Tab
    {
        Cards,
        Overview,
        Suggestions
    }

    #endregion
    
    #region Statements

    [Parameter, EditorRequired] public required string DeckCode { get; set; }

    protected DeckDetails? DeckDetails;
    protected IReadOnlyList<TCGPCard> TCGPCards { get; set; } = [];
    protected IReadOnlyList<TCGPCard> TCGPHighlightedCards { get; set; } = [];
    
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
    
    protected Mode CurrentMode { get; set; } = Mode.View;
    protected Tab CurrentTab { get; set; } = Tab.Cards;
    
    protected string? AuthenticatedUserUuid;
    
    [Inject] private IJSRuntime _js { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckDetailsService _deckDetailsService { get; set; } = null!;
    [Inject] private ITCGPCardRequester _tcgpCardRequester { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        DeckDetails? deckDetails = await _deckDetailsService.GetByCodeAsync(DeckCode);
        
        if (deckDetails == null)
        {
            //_navigationManager.NavigateTo("/", true);
            return;
        }
        
        DeckDetails = deckDetails;

        AuthenticatedUserState authenticatedUser = UIStore.GetState<AuthenticatedUserState>();
        if (authenticatedUser.Id > 0)
        {
            AuthenticatedUserUuid = authenticatedUser.Uuid;
        }
        
        List<TCGPCardRequest> tcgpCardRequests = DeckDetails.Cards
            .Select(cr => new TCGPCardRequest(cr.CollectionCode, cr.CollectionNumber))
            .ToList();
        
        TCGPCardsRequest deckRequest = new(tcgpCardRequests);
        TCGPCards = await _tcgpCardRequester.GetTCGPCardsByRequestAsync(deckRequest);
        
        TCGPHighlightedCards = TCGPCards
            .Where(c => DeckDetails.HighlightedCards
                .Any(dc => dc.CollectionCode == c.Collection.Code && dc.CollectionNumber == c.CollectionNumber))
            .ToList();
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
        if (DeckDetails == null) 
            return;
        
        await _js.InvokeVoidAsync("navigator.clipboard.writeText", DeckDetails.Code);
        DeckCode = Localizer.Localize("feedback.text.copied");
        StateHasChanged();

        await Task.Delay(1500);
        DeckCode = DeckDetails.Code;
        StateHasChanged();
    }
    
    protected void SelectTab(Tab tab)
    {
        CurrentTab = tab;
    }
    
    protected void SwitchTab()
    {
        CurrentTab = CurrentTab switch
        {
            Tab.Cards => Tab.Overview,
            Tab.Overview => Tab.Cards,
            _ => CurrentTab
        };
    }
    
    protected void EditDeck()
    {
        Console.WriteLine("EditDeck clicked");
        //_navigationManager.NavigateTo($"/decks/{DeckDetails?.Code}/edit");
    }

    #endregion
}