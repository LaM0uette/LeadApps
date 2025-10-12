using TopDeck.Api.Entities;
using TopDeck.Api.Repositories;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class VoteService : IVoteService
{
    #region Statements

    private readonly IVoteRepository _repo;
    private readonly IDeckItemRepository _deckItems;
    private readonly IDeckSuggestionRepository _deckSuggestions;
    private readonly IUserRepository _users;

    public VoteService(IVoteRepository repo, IDeckItemRepository deckItems, IDeckSuggestionRepository deckSuggestions, IUserRepository users)
    {
        _repo = repo;
        _deckItems = deckItems;
        _deckSuggestions = deckSuggestions;
        _users = users;
    }

    #endregion

    #region IService

    public async Task<bool> VoteDeckAsync(DeckVoteInputDTO dto, CancellationToken ct = default)
    {
        Deck? deckItem = await _deckItems.GetByIdAsync(dto.Id, true, ct);
        if (deckItem is null)
            throw new InvalidOperationException($"Deck with id {dto.Id} not found");
        
        if (!Guid.TryParse(dto.UserUuid, out Guid userUuid))
            throw new InvalidOperationException($"User uuid {dto.UserUuid} is not a valid GUID");

        User? user = await _users.GetByUuidAsync(userUuid, ct);
        if (user is null)
            throw new InvalidOperationException($"User with uuid {userUuid} not found");

        if (!dto.IsLike)
        {
            // Mutual exclusion: remove existing like, if any
            await _repo.DeleteDeckLikeAsync(dto.Id, user.Id, ct);
            
            await _repo.AddDeckDislikeAsync(new DeckDislike
            {
                DeckId = dto.Id,
                Deck = null!,
                UserId = user.Id,
                User = null!
            }, ct);

            return true;
        }
        
        DeckLike? existing = await _repo.GetDeckLikeByIdAsync(dto.Id, user.Id, ct);
        if (existing is not null)
            return true;

        // Mutual exclusion: remove existing dislike, if any
        await _repo.DeleteDeckDislikeAsync(dto.Id, user.Id, ct);

        await _repo.AddDeckLikeAsync(new DeckLike
        {
            DeckId = dto.Id,
            Deck = null!,
            UserId = user.Id,
            User = null!
        }, ct);

        return true;

    }

    public async Task<bool> VoteDeckSuggestionAsync(DeckSuggestionVoteInputDTO dto, CancellationToken ct = default)
    {
        DeckSuggestion? deckSuggestion = await _deckSuggestions.GetByIdAsync(dto.Id, true, ct);
        if (deckSuggestion is null)
            throw new InvalidOperationException($"Deck suggestion with id {dto.Id} not found");
        
        if (!Guid.TryParse(dto.UserUuid, out Guid userUuid))
            throw new InvalidOperationException($"User uuid {dto.UserUuid} is not a valid GUID");

        User? user = await _users.GetByUuidAsync(userUuid, ct);
        if (user is null)
            throw new InvalidOperationException($"User with uuid {userUuid} not found");

        if (!dto.IsLike)
        {
            // Mutual exclusion: remove existing like, if any
            await _repo.DeleteDeckSuggestionLikeAsync(dto.Id, user.Id, ct);
            
            await _repo.AddDeckSuggestionDislikeAsync(new DeckSuggestionDislike
            {
                DeckSuggestionId = dto.Id,
                Suggestion = null!,
                UserId = user.Id,
                User = null!
            }, ct);
            
            return true;
        }
        
        DeckSuggestionLike? existing = await _repo.GetDeckSuggestionLikeByIdAsync(dto.Id, user.Id, ct);
        if (existing is not null)
            return true;
        
        // Mutual exclusion: remove existing dislike, if any
        await _repo.DeleteDeckSuggestionDislikeAsync(dto.Id, user.Id, ct);
        
        await _repo.AddDeckSuggestionLikeAsync(new DeckSuggestionLike
        {
            DeckSuggestionId = dto.Id,
            Suggestion = null!,
            UserId = user.Id,
            User = null!
        }, ct);
        
        return true;
    }

    #endregion
}