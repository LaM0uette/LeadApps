using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckDislikeService : IDeckDislikeService
{
    private readonly IDeckDislikeRepository _dislikes;
    private readonly IDeckLikeRepository _likes;
    private readonly IDeckRepository _decks;
    private readonly IUserRepository _users;

    public DeckDislikeService(IDeckDislikeRepository dislikes, IDeckLikeRepository likes, IDeckRepository decks, IUserRepository users)
    {
        _dislikes = dislikes;
        _likes = likes;
        _decks = decks;
        _users = users;
    }

    public async Task<DeckDislikeOutputDTO?> CreateAsync(DeckDislikeInputDTO dto, CancellationToken ct = default)
    {
        // Validate FKs
        if (await _decks.GetByIdAsync(dto.DeckId, includeRelations: true, ct) is not Deck deck)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");
        if (await _users.GetByIdAsync(dto.UserId, ct) is not User user)
            throw new InvalidOperationException($"User with id {dto.UserId} not found");

        // Idempotent
        DeckDislike? existing = await _dislikes.GetByIdAsync(dto.DeckId, dto.UserId, ct);
        if (existing is not null)
        {
            return new DeckDislikeOutputDTO(deck.ToShallowOutput(), user.ToOutput());
        }

        // Mutual exclusion: remove existing like, if any
        await _likes.DeleteAsync(dto.DeckId, dto.UserId, ct);

        await _dislikes.AddAsync(new DeckDislike
        {
            DeckId = dto.DeckId,
            Deck = null!,
            UserId = dto.UserId,
            User = null!
        }, ct);

        return new DeckDislikeOutputDTO(deck.ToShallowOutput(), user.ToOutput());
    }

    public async Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default)
    {
        return await _dislikes.DeleteAsync(deckId, userId, ct);
    }
}
