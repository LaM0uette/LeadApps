using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IVoteRepository
{
    Task<DeckLike?> GetDeckLikeByIdAsync(int deckId, int userId, CancellationToken ct = default);
    Task<DeckSuggestionLike?> GetDeckSuggestionLikeByIdAsync(int suggestionId, int userId, CancellationToken ct = default);
    
    Task<DeckLike> AddDeckLikeAsync(DeckLike like, CancellationToken ct = default);
    Task<bool> DeleteDeckLikeAsync(int deckId, int userId, CancellationToken ct = default);
    Task<DeckDislike> AddDeckDislikeAsync(DeckDislike dislike, CancellationToken ct = default);
    Task<bool> DeleteDeckDislikeAsync(int deckId, int userId, CancellationToken ct = default);
    
    Task<DeckSuggestionLike> AddDeckSuggestionLikeAsync(DeckSuggestionLike like, CancellationToken ct = default);
    Task<bool> DeleteDeckSuggestionLikeAsync(int suggestionId, int userId, CancellationToken ct = default);
    Task<DeckSuggestionDislike> AddDeckSuggestionDislikeAsync(DeckSuggestionDislike dislike, CancellationToken ct = default);
    Task<bool> DeleteDeckSuggestionDislikeAsync(int suggestionId, int userId, CancellationToken ct = default);
}