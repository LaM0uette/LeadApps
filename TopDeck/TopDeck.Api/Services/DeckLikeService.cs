using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckLikeService : IDeckLikeService
{
    private readonly IDeckLikeRepository _likes;
    private readonly IDeckDislikeRepository _dislikes;
    private readonly IDeckItemRepository _deckItems;
    private readonly IUserRepository _users;

    public DeckLikeService(IDeckLikeRepository likes, IDeckDislikeRepository dislikes, IDeckItemRepository deckItems, IUserRepository users)
    {
        _likes = likes;
        _dislikes = dislikes;
        _deckItems = deckItems;
        _users = users;
    }

    public async Task<DeckLikeOutputDTO?> CreateAsync(DeckLikeInputDTO dto, CancellationToken ct = default)
    {
        // Validate FKs
        if (await _deckItems.GetByIdAsync(dto.DeckId, true, ct) is not Deck deck)
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