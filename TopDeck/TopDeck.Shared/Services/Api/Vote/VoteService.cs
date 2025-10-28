using TopDeck.Contracts.DTO;

namespace TopDeck.Shared.Services;

public class VoteService : ApiService, IVoteService
{
    #region Statements

    private const string _route = "vote";
    
    public VoteService(HttpClient http) : base(http) { }

    #endregion

    #region IDeckReactionService

    public async Task<bool> VoteDeckAsync(DeckVoteInputDTO dto, CancellationToken ct = default)
    {
        return await PostJsonAsync<DeckVoteInputDTO, bool>($"{_route}/deck", dto, ct);
    }
    
    public async Task<bool> VoteDeckSuggestionAsync(DeckSuggestionVoteInputDTO dto, CancellationToken ct = default)
    {
        return await PostJsonAsync<DeckSuggestionVoteInputDTO, bool>($"{_route}/deckSuggestion", dto, ct);
    }

    #endregion
    
}