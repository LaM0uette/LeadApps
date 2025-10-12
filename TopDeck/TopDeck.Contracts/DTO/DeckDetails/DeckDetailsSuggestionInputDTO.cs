namespace TopDeck.Contracts.DTO;

public record DeckDetailsSuggestionInputDTO(
    int SuggestorId,
    IEnumerable<DeckDetailsCardInputDTO> AddedCards,
    IEnumerable<DeckDetailsCardInputDTO> RemovedCards,
    IEnumerable<int> AddedEnergyIds,
    IEnumerable<int> RemovedEnergyIds
);
