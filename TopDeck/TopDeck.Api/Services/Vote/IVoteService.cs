using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public interface IVoteService
{
    Task<bool> VoteDeckAsync(DeckVoteInputDTO dto, CancellationToken ct = default);
    Task<bool> VoteDeckSuggestionAsync(DeckSuggestionVoteInputDTO dto, CancellationToken ct = default);
}