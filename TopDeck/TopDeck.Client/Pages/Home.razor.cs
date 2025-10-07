using System.Security.Claims;
using LocalizedComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Requesters.AuthUser;
using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages;

public class HomeBase : LocalizedComponentBase
{
    #region Statements
    
    protected IReadOnlyList<Deck> Decks { get; set; } = [];

    [Inject] private IDeckService _deckService { get; set; } = null!;
    [Inject] private IAuthUserRequester _authUserRequester { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        Decks = await _deckService.GetAllAsync();

        User? user = await _authUserRequester.GetAuthenticatedUserAsync();
        
        if (user is not null)
        {
            Console.WriteLine(user.OAuthId + " " + user.UserName);
        }
    }

    #endregion

    #region Methods

    //

    #endregion
}