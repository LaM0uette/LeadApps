using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Shared.Components;

public class VotePanelBase : ComponentBase
{
    #region Statements

    [Parameter] public int? DeckId { get; set; }
    [Parameter] public int? SuggestionId { get; set; }
    [Parameter] public IReadOnlyCollection<User> UserLikes { get; set; } = [];
    [Parameter] public IReadOnlyCollection<User> UserDislikes { get; set; } = [];
    [Parameter, EditorRequired] public required string Width { get; set; } = "100px";
    [Parameter, EditorRequired] public required string Height { get; set; } = "26px";
    [Parameter, EditorRequired] public required string FontSize { get; set; } = "0.63em";
    
    protected string LikeCountFormatted => Format(UserLikes.Count);
    
    protected bool IsLiked;
    protected bool IsDisliked;
    
    [Inject] private UIStore.UIStore _uiStore { get; set; } = null!;
    [Inject] private IDeckReactionService _deckReactionService { get; set; } = null!;

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
    
    #endregion

    #region Methods

    protected async Task OnLikeClicked()
    {
        string? userOAuthId = _uiStore.GetState<AuthenticatedUserState>().OAuthId;
        
        if (userOAuthId == null)
            return;
        
        IsLiked = !IsLiked;
        IsDisliked = false;

        int userId = _uiStore.GetState<AuthenticatedUserState>().Id;

        if (DeckId is not null && SuggestionId is null)
        {
            await _deckReactionService.LikeDeckAsync(DeckId.Value, userId, IsLiked);
        }
        else if (SuggestionId is not null && DeckId is null)
        {
            await _deckReactionService.LikeSuggestionAsync(SuggestionId.Value, userId, IsLiked);
        }
        else
        {
            throw new InvalidOperationException("Either DeckId or SuggestionId must be set, but not both.");
        }
        
        if (IsLiked)
        {
            UserLikes = UserLikes.Append(new User(userId, "fakeUser", userOAuthId, "fakeUser", DateTime.Now)).ToList();
            UserDislikes = UserDislikes.Where(u => u.Id != userId).ToList();
        }
        else
        {
            UserLikes = UserLikes.Where(u => u.Id != userId).ToList();
        }
        
        StateHasChanged();
    }
    
    protected async Task OnDislikeClicked()
    {
        string? userOAuthId = _uiStore.GetState<AuthenticatedUserState>().OAuthId;
        
        if (userOAuthId == null)
            return;
        
        IsDisliked = !IsDisliked;
        IsLiked = false;
        
        int userId = _uiStore.GetState<AuthenticatedUserState>().Id;
        
        if (DeckId is not null && SuggestionId is null)
        {
            await _deckReactionService.DislikeDeckAsync(DeckId.Value, userId, IsDisliked);
        }
        else if (SuggestionId is not null && DeckId is null)
        {
            await _deckReactionService.DislikeSuggestionAsync(SuggestionId.Value, userId, IsDisliked);
        }
        else
        {
            throw new InvalidOperationException("Either DeckId or SuggestionId must be set, but not both.");
        }
        
        if (IsDisliked)
        {
            UserDislikes = UserDislikes.Append(new User(userId, "fakeUser", userOAuthId, "fakeUser", DateTime.Now)).ToList();
            UserLikes = UserLikes.Where(u => u.Id != userId).ToList();
        }
        else
        {
            UserDislikes = UserDislikes.Where(u => u.Id != userId).ToList();
        }
        
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