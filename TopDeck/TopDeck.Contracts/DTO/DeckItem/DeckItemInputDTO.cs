namespace TopDeck.Contracts.DTO;

public record DeckItemInputDTO(
    int CreatorId,
    string Name,
    ICollection<DeckCardInputDTO> Cards,
    ICollection<int> EnergyIds,
    ICollection<int> TagIds
);