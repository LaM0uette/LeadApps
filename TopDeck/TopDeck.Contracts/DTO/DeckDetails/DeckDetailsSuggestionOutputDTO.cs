namespace TopDeck.Contracts.DTO;

public record DeckDetailsSuggestionOutputDTO(
    int Id,
    string SuggestorUuid,
    IEnumerable<DeckDetailsCardOutputDTO> AddedCards,
    IEnumerable<DeckDetailsCardOutputDTO> RemovedCards,
    IEnumerable<int> AddedEnergyIds,
    IEnumerable<int> RemovedEnergyIds,
    IEnumerable<string> LikeUserUuids,
    IEnumerable<string> DislikeUserUuids,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
