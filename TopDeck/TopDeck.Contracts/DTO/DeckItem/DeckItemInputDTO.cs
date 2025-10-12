namespace TopDeck.Contracts.DTO;

public record DeckItemInputDTO(
    int CreatorId,
    string Name,
    ICollection<DeckItemCardInputDTO> Cards,
    ICollection<int> EnergyIds,
    ICollection<int> TagIds
);