using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Shared.Components;

public class LikeButtonBase : ComponentBase
{
    #region Statements

    [Parameter] public IReadOnlyCollection<User> UserLikes { get; set; } = [];
    [Parameter] public IReadOnlyCollection<User> UserDislikes { get; set; } = [];
    
    protected string LikeCountFormatted => Format(UserLikes.Count);
    
    protected bool IsLiked;
    protected bool IsDisliked;
    
    [Inject] private UIStore.UIStore _uiStore { get; set; } = null!;

    #endregion

    #region Methods
    
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) 
            return;
        
        AuthenticatedUserState currentUserState = _uiStore.GetState<AuthenticatedUserState>();
        
        IsLiked = UserLikes.Any(u => u.OAuthId == currentUserState.OAuthId);
        IsDisliked = UserDislikes.Any(u => u.OAuthId == currentUserState.OAuthId);
        
        if (IsLiked && IsDisliked)
            throw new InvalidOperationException("A user cannot both like and dislike at the same time."); // TODO: Log this instead of throwing
            
        StateHasChanged();
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