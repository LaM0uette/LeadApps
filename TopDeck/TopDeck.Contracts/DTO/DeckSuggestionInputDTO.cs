namespace TopDeck.Contracts.DTO;

public record DeckSuggestionInputDTO(
    int SuggestorId,
    int DeckId,
    ICollection<int> AddedCardIds,
    ICollection<int> RemovedCardIds,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds
);