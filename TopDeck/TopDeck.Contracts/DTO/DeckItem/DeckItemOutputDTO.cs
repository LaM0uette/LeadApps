namespace TopDeck.Contracts.DTO;

public record DeckItemOutputDTO(
    int Id,
    string CreatorUuid,
    string Name,
    string Code,
    IEnumerable<DeckItemCardOutputDTO> HighlightedCards,
    IEnumerable<int> EnergyIds,
    IEnumerable<int> TagIds,
    IEnumerable<string> LikeUserUuids,
    IEnumerable<string> DislikeUserUuids,
    DateTime CreatedAt
);