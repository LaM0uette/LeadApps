using Microsoft.AspNetCore.Components;
using TopDeck.Contracts.DTO;
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
    [Inject] private IVoteService _voteService { get; set; } = null!;
    
    private bool _userAuthenticated;
    private string _userUuid = string.Empty;

    protected override void OnInitialized()
    {
        AuthenticatedUserState currentUserState = _uiStore.GetState<AuthenticatedUserState>();
            
        int userId = currentUserState.Id;
        string userUuid = currentUserState.Uuid;
            
        if (userId == -1 || string.IsNullOrWhiteSpace(userUuid))
        {
            _userAuthenticated = false;
            return;
        }
        
        _userUuid = userUuid;
        _userAuthenticated = true;
    }

    protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) 
                return;
            
            IsLiked = LikeUserUuids.Any(s => s == _userUuid);
            IsDisliked = DislikeUserUuids.Any(s => s == _userUuid);
            
            if (IsLiked && IsDisliked)
                throw new InvalidOperationException("A user cannot both like and dislike at the same time."); // TODO: Log this instead of throwing
                
            StateHasChanged();
        }
    
    #endregion

    #region Methods

    protected async Task OnLikeClicked()
    {
        if (!_userAuthenticated || IsLiked)
            return;
        
        IsLiked = true;
        IsDisliked = false;
        
        if (IsLiked)
        {
            LikeUserUuids = LikeUserUuids.Append(_userUuid).ToList();
            DislikeUserUuids = DislikeUserUuids.Where(s => s != _userUuid).ToList();
        }
        else
        {
            LikeUserUuids = LikeUserUuids.Where(s => s != _userUuid).ToList();
        }
        
        StateHasChanged();

        if (DeckId is not null && SuggestionId is null)
        {
            DeckVoteInputDTO dto = new(DeckId.Value, _userUuid, true);
            await _voteService.VoteDeckAsync(dto);
        }
        else if (SuggestionId is not null && DeckId is null)
        {
            DeckSuggestionVoteInputDTO dto = new(SuggestionId.Value, _userUuid, true);
            await _voteService.VoteDeckSuggestionAsync(dto);
        }
        else
        {
            throw new InvalidOperationException("Either DeckId or SuggestionId must be set, but not both.");
        }
    }
    
    protected async Task OnDislikeClicked()
    {
        if (!_userAuthenticated || IsDisliked)
            return;
        
        IsDisliked = true;
        IsLiked = false;
        
        if (IsDisliked)
        {
            DislikeUserUuids = DislikeUserUuids.Append(_userUuid).ToList();
            LikeUserUuids = LikeUserUuids.Where(s => s != _userUuid).ToList();
        }
        else
        {
            DislikeUserUuids = DislikeUserUuids.Where(s => s != _userUuid).ToList();
        }
        
        StateHasChanged();
        
        if (DeckId is not null && SuggestionId is null)
        {
            DeckVoteInputDTO dto = new(DeckId.Value, _userUuid, false);
            await _voteService.VoteDeckAsync(dto);
        }
        else if (SuggestionId is not null && DeckId is null)
        {
            DeckSuggestionVoteInputDTO dto = new(SuggestionId.Value, _userUuid, false);
            await _voteService.VoteDeckSuggestionAsync(dto);
        }
        else
        {
            throw new InvalidOperationException("Either DeckId or SuggestionId must be set, but not both.");
        }
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