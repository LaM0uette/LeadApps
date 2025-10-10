namespace TopDeck.Contracts.DTO;

public record DeckSuggestionInputDTO(
    int SuggestorId,
    int DeckId,
    ICollection<DeckCardInputDTO> AddedCards,
    ICollection<DeckCardInputDTO> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds
);