using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckLikeService : IDeckLikeService
{
    private readonly IDeckLikeRepository _likes;
    private readonly IDeckDislikeRepository _dislikes;
    private readonly IDeckRepository _decks;
    private readonly IUserRepository _users;

    public DeckLikeService(IDeckLikeRepository likes, IDeckDislikeRepository dislikes, IDeckRepository decks, IUserRepository users)
    {
        _likes = likes;
        _dislikes = dislikes;
        _decks = decks;
        _users = users;
    }

    public async Task<DeckLikeOutputDTO?> CreateAsync(DeckLikeInputDTO dto, CancellationToken ct = default)
    {
        // Validate FKs
        if (await _decks.GetByIdAsync(dto.DeckId, includeRelations: true, ct) is not Deck deck)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");
        if (await _users.GetByIdAsync(dto.UserId, ct) is not User user)
            throw new InvalidOperationException($"User with id {dto.UserId} not found");

        // Idempotent: if like exists, return current representation
        DeckLike? existing = await _likes.GetByIdAsync(dto.DeckId, dto.UserId, ct);
        if (existing is not null)
        {
            DeckLikeOutputDTO output = new(deck.ToShallowOutput(), user.ToOutput());
            return output;
        }

        // Mutual exclusion: remove existing dislike, if any
        await _dislikes.DeleteAsync(dto.DeckId, dto.UserId, ct);

        await _likes.AddAsync(new DeckLike
        {
            DeckId = dto.DeckId,
            Deck = null!,
            UserId = dto.UserId,
            User = null!
        }, ct);

        // Return DTO
        return new DeckLikeOutputDTO(deck.ToShallowOutput(), user.ToOutput());
    }

    public async Task<bool> DeleteAsync(int deckId, int userId, CancellationToken ct = default)
    {
        return await _likes.DeleteAsync(deckId, userId, ct);
    }
}