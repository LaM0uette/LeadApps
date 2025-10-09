namespace TopDeck.Contracts.DTO;

public record DeckSuggestionInputDTO(
    int SuggestorId,
    int DeckId,
    ICollection<CardRefDTO> AddedCards,
    ICollection<CardRefDTO> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds
);