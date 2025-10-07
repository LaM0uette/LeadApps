using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class LikeButtonBase : ComponentBase
{
    #region Statements

    [Parameter] public int Likes { get; set; }
    [Parameter] public User? User { get; set; }
    
    protected bool IsLiked;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        if (User is null)
            return;
        
        Console.WriteLine(User.OAuthId + " " + User.UserName);
    }

    #endregion
}