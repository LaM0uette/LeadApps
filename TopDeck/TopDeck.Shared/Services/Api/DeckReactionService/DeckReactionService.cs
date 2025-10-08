using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

// TODO: Need to cleen/refactor this service and the related components
public class DeckReactionService : ApiService, IDeckReactionService
{
    private const string LikesRoute = "/api/deck-likes";
    private const string DislikesRoute = "/api/deck-dislikes";

    public async Task<bool> LikeAsync(int deckId, int userId, bool on, CancellationToken ct = default)
    {
        if (on)
        {
            // Create (idempotent on server)
            var payload = new DeckLikeInputDTO(deckId, userId);
            DeckLikeOutputDTO? result = await PostJsonAsync<DeckLikeInputDTO, DeckLikeOutputDTO>(LikesRoute, payload, ct);
            return result is not null;
        }
        else
        {
            // Delete
            return await DeleteAsync($"{LikesRoute}/{deckId}/{userId}", ct);
        }
    }

    public async Task<bool> DislikeAsync(int deckId, int userId, bool on, CancellationToken ct = default)
    {
        if (on)
        {
            var payload = new DeckDislikeInputDTO(deckId, userId);
            DeckDislikeOutputDTO? result = await PostJsonAsync<DeckDislikeInputDTO, DeckDislikeOutputDTO>(DislikesRoute, payload, ct);
            return result is not null;
        }
        else
        {
            return await DeleteAsync($"{DislikesRoute}/{deckId}/{userId}", ct);
        }
    }
}