namespace TopDeck.Contracts.DTO;

public record DeckSuggestionInputDTO(
    int SuggestorId,
    int DeckId,
    ICollection<DeckDetailsCardOutputDTO> AddedCards,
    ICollection<DeckDetailsCardOutputDTO> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds
);