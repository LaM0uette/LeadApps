namespace TopDeck.Contracts.DTO;

public record DeckInputDTO(
    int CreatorId,
    string Name,
    ICollection<CardRefDTO> Cards,
    ICollection<int> EnergyIds
);