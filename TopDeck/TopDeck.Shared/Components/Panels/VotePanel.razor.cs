using Microsoft.AspNetCore.Components;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore.States.AuthenticatedUser;

namespace TopDeck.Shared.Components;

public class VotePanelBase : ComponentBase
{
    #region Statements

    [Parameter, EditorRequired] public required string Width { get; set; } = "100px";
    [Parameter, EditorRequired] public required string Height { get; set; } = "26px";
    [Parameter, EditorRequired] public required string FontSize { get; set; } = "0.63em";
    
    [Parameter] public int? DeckId { get; set; }
    [Parameter] public int? SuggestionId { get; set; }
    [Parameter] public IReadOnlyList<string> LikeUserUuids { get; set; } = [];
    [Parameter] public IReadOnlyList<string> DislikeUserUuids { get; set; } = [];
    
    protected string LikeCountFormatted => Format(LikeUserUuids.Count);
    
    protected bool IsLiked;
    protected bool IsDisliked;
    
    [Inject] private UIStore.UIStore _uiStore { get; set; } = null!;
    [Inject] private IDeckReactionService _deckReactionService { get; set; } = null!;

    protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) 
                return;
            
            AuthenticatedUserState currentUserState = _uiStore.GetState<AuthenticatedUserState>();
            
            IsLiked = LikeUserUuids.Any(s => s == currentUserState.Uuid);
            IsDisliked = DislikeUserUuids.Any(s => s == currentUserState.Uuid);
            
            if (IsLiked && IsDisliked)
                throw new InvalidOperationException("A user cannot both like and dislike at the same time."); // TODO: Log this instead of throwing
                
            StateHasChanged();
        }
    
    #endregion

    #region Methods

    protected async Task OnLikeClicked()
    {
        string? userUuid = _uiStore.GetState<AuthenticatedUserState>().Uuid;
        
        if (userUuid == null)
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
            LikeUserUuids = LikeUserUuids.Append(userUuid).ToList();
            DislikeUserUuids = DislikeUserUuids.Where(s => s != userUuid).ToList();
        }
        else
        {
            LikeUserUuids = LikeUserUuids.Where(s => s != userUuid).ToList();
        }
        
        StateHasChanged();
    }
    
    protected async Task OnDislikeClicked()
    {
        string? userUuid = _uiStore.GetState<AuthenticatedUserState>().Uuid;
        
        if (userUuid == null)
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
            DislikeUserUuids = DislikeUserUuids.Append(userUuid).ToList();
            LikeUserUuids = LikeUserUuids.Where(s => s != userUuid).ToList();
        }
        else
        {
            DislikeUserUuids = DislikeUserUuids.Where(s => s != userUuid).ToList();
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