using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

// TODO: Need to cleen/refactor this service and the related components
public class DeckReactionService : ApiService, IDeckReactionService
{
    #region Statements

    private const string LikesRoute = "/api/deck-likes";
    private const string DislikesRoute = "/api/deck-dislikes";
    private const string SuggestionLikesRoute = "/api/suggestion-likes";
    private const string SuggestionDislikesRoute = "/api/suggestion-dislikes";

    #endregion

    #region IDeckReactionService

    public async Task<bool> LikeDeckAsync(int deckId, int userId, bool on, CancellationToken ct = default)
    {
        if (!on)
        {
            return await DeleteAsync($"{LikesRoute}/{deckId}/{userId}", ct);
        }
        
        DeckLikeInputDTO payload = new(deckId, userId);
        DeckLikeOutputDTO? result = await PostJsonAsync<DeckLikeInputDTO, DeckLikeOutputDTO>(LikesRoute, payload, ct);
        return result is not null;

    }

    public async Task<bool> DislikeDeckAsync(int deckId, int userId, bool on, CancellationToken ct = default)
    {
        if (!on)
        {
            return await DeleteAsync($"{DislikesRoute}/{deckId}/{userId}", ct);
        }
        
        DeckDislikeInputDTO payload = new(deckId, userId);
        DeckDislikeOutputDTO? result = await PostJsonAsync<DeckDislikeInputDTO, DeckDislikeOutputDTO>(DislikesRoute, payload, ct);
        return result is not null;

    }
    
    public async Task<bool> LikeSuggestionAsync(int suggestionId, int userId, bool on, CancellationToken ct = default)
    {
        if (!on)
        {
            return await DeleteAsync($"{SuggestionLikesRoute}/{suggestionId}/{userId}", ct);
        }
        
        DeckSuggestionLikeInputDTO payload = new(suggestionId, userId);
        DeckSuggestionLikeOutputDTO? result = await PostJsonAsync<DeckSuggestionLikeInputDTO, DeckSuggestionLikeOutputDTO>(SuggestionLikesRoute, payload, ct);
        return result is not null;
    }
    
    public async Task<bool> DislikeSuggestionAsync(int suggestionId, int userId, bool on, CancellationToken ct = default)
    {
        if (!on)
        {
            return await DeleteAsync($"{SuggestionDislikesRoute}/{suggestionId}/{userId}", ct);
        }
        
        DeckSuggestionDislikeInputDTO payload = new(suggestionId, userId);
        DeckSuggestionDislikeOutputDTO? result = await PostJsonAsync<DeckSuggestionDislikeInputDTO, DeckSuggestionDislikeOutputDTO>(SuggestionDislikesRoute, payload, ct);
        return result is not null;
    }

    #endregion
    
}