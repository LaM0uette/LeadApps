using Microsoft.AspNetCore.Components;

namespace TopDeck.Shared.Components;

public class LikeButtonBase : ComponentBase
{
    #region Statements

    [Parameter] public int Likes { get; set; }
    
    protected bool IsLiked;

    #endregion

    #region Methods

    

    #endregion
}