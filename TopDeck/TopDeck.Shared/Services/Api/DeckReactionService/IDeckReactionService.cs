namespace TopDeck.Shared.Services;

public interface IDeckReactionService
{
    Task<bool> LikeDeckAsync(int deckId, int userId, bool on, CancellationToken ct = default);
    Task<bool> DislikeDeckAsync(int deckId, int userId, bool on, CancellationToken ct = default);
    
    Task<bool> LikeSuggestionAsync(int suggestionId, int userId, bool on, CancellationToken ct = default);
    Task<bool> DislikeSuggestionAsync(int suggestionId, int userId, bool on, CancellationToken ct = default);
}