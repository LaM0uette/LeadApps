using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckSuggestionLikeService : IDeckSuggestionLikeService
{
    private readonly IDeckSuggestionLikeRepository _likes;
    private readonly IDeckSuggestionDislikeRepository _dislikes;
    private readonly IDeckSuggestionRepository _suggestions;
    private readonly IUserRepository _users;

    public DeckSuggestionLikeService(IDeckSuggestionLikeRepository likes, IDeckSuggestionDislikeRepository dislikes, IDeckSuggestionRepository suggestions, IUserRepository users)
    {
        _likes = likes;
        _dislikes = dislikes;
        _suggestions = suggestions;
        _users = users;
    }

    public async Task<DeckSuggestionLikeOutputDTO?> CreateAsync(DeckSuggestionLikeInputDTO dto, CancellationToken ct = default)
    {
        // Validate FKs
        if (await _suggestions.GetByIdAsync(dto.DeckSuggestionId, includeRelations: true, ct) is not DeckSuggestion suggestion)
            throw new InvalidOperationException($"DeckSuggestion with id {dto.DeckSuggestionId} not found");
        if (await _users.GetByIdAsync(dto.UserId, ct) is not User user)
            throw new InvalidOperationException($"User with id {dto.UserId} not found");

        // Idempotent
        DeckSuggestionLike? existing = await _likes.GetByIdAsync(dto.DeckSuggestionId, dto.UserId, ct);
        if (existing is not null)
        {
            return new DeckSuggestionLikeOutputDTO(suggestion.ToShallowOutput(), user.ToOutput());
        }

        // Mutual exclusion: remove existing dislike, if any
        await _dislikes.DeleteAsync(dto.DeckSuggestionId, dto.UserId, ct);

        await _likes.AddAsync(new DeckSuggestionLike
        {
            DeckSuggestionId = dto.DeckSuggestionId,
            Suggestion = null!,
            UserId = dto.UserId,
            User = null!
        }, ct);

        return new DeckSuggestionLikeOutputDTO(suggestion.ToShallowOutput(), user.ToOutput());
    }

    public async Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        return await _likes.DeleteAsync(deckSuggestionId, userId, ct);
    }
}