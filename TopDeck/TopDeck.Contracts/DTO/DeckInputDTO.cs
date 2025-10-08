namespace TopDeck.Contracts.DTO;

public record DeckInputDTO(
    int CreatorId,
    string Name,
    ICollection<int> CardIds,
    ICollection<int> EnergyIds
);