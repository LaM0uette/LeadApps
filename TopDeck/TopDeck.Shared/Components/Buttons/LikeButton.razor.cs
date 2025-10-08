using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Components;

public class LikeButtonBase : ComponentBase
{
    #region Statements

    [Parameter] public int Likes { get; set; }
    [Parameter] public User? User { get; set; }
    
    protected string LikeCountFormatted => Format(Likes);
    protected bool IsLiked;
    protected bool IsDisliked;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        if (User is null)
            return;
        
        Console.WriteLine(User.OAuthId + " " + User.UserName);
    }
    
    
    private string Format(int count)
    {
        return count switch
        {
            >= 1_000_000 => (count / 1_000_000D).ToString("0.#") + "M",
            >= 1_000 => (count / 1_000D).ToString("0.#") + "k",
            _ => count.ToString()
        };
    }

    #endregion
}