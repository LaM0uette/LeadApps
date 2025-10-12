namespace TopDeck.Contracts.DTO;

public record DeckSuggestionInputDTO(
    int SuggestorId,
    int DeckId,
    ICollection<DeckDetailsCardInputDTO> AddedCards,
    ICollection<DeckDetailsCardInputDTO> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds
);