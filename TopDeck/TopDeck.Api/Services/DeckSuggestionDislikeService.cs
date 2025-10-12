using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckSuggestionDislikeService : IDeckSuggestionDislikeService
{
    private readonly IDeckSuggestionDislikeRepository _dislikes;
    private readonly IDeckSuggestionLikeRepository _likes;
    private readonly IDeckSuggestionRepository _suggestions;
    private readonly IUserRepository _users;

    public DeckSuggestionDislikeService(IDeckSuggestionDislikeRepository dislikes, IDeckSuggestionLikeRepository likes, IDeckSuggestionRepository suggestions, IUserRepository users)
    {
        _dislikes = dislikes;
        _likes = likes;
        _suggestions = suggestions;
        _users = users;
    }

    public async Task<DeckSuggestionDislikeOutputDTO?> CreateAsync(DeckSuggestionDislikeInputDTO dto, CancellationToken ct = default)
    {
        if (await _suggestions.GetByIdAsync(dto.DeckSuggestionId, includeRelations: true, ct) is not DeckSuggestion suggestion)
            throw new InvalidOperationException($"DeckSuggestion with id {dto.DeckSuggestionId} not found");
        if (await _users.GetByIdAsync(dto.UserId, ct) is not User user)
            throw new InvalidOperationException($"User with id {dto.UserId} not found");

        DeckSuggestionDislike? existing = await _dislikes.GetByIdAsync(dto.DeckSuggestionId, dto.UserId, ct);
        if (existing is not null)
        {
            return new DeckSuggestionDislikeOutputDTO(suggestion.ToShallowOutput(), user.ToOutput());
        }

        // Mutual exclusion: remove existing like, if any
        await _likes.DeleteAsync(dto.DeckSuggestionId, dto.UserId, ct);

        await _dislikes.AddAsync(new DeckSuggestionDislike
        {
            DeckSuggestionId = dto.DeckSuggestionId,
            Suggestion = null!,
            UserId = dto.UserId,
            User = null!
        }, ct);

        return new DeckSuggestionDislikeOutputDTO(suggestion.ToShallowOutput(), user.ToOutput());
    }

    public async Task<bool> DeleteAsync(int deckSuggestionId, int userId, CancellationToken ct = default)
    {
        return await _dislikes.DeleteAsync(deckSuggestionId, userId, ct);
    }
}
