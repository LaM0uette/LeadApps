namespace TopDeck.Contracts.DTO;

public record DeckInputDTO(
    int CreatorId,
    string Name,
    string Code,
    ICollection<int> CardIds,
    ICollection<int> EnergyIds
);