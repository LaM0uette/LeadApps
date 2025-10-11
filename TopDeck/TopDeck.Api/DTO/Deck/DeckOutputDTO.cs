namespace TopDeck.Api.DTO;

public record DeckOutputDTO(
    int Id,
    string CreatorId,
    string Name,
    string Code,
    IEnumerable<DeckCardOutputDTO> HighlightedCards,
    IEnumerable<int> EnergyIds,
    IEnumerable<int> TagIds,
    IEnumerable<string> LikeUserIds,
    IEnumerable<string> DislikeUserIds,
    DateTime CreatedAt
);