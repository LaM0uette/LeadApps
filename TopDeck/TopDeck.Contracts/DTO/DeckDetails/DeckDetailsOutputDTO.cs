namespace TopDeck.Contracts.DTO;

public record DeckDetailsOutputDTO(
    int Id,
    string CreatorUuid,
    string Name,
    string Code,
    IEnumerable<DeckDetailsCardOutputDTO> Cards,
    IEnumerable<DeckDetailsCardOutputDTO> HighlightedCards,
    IEnumerable<int> EnergyIds,
    IEnumerable<int> TagIds,
    IEnumerable<string> LikeUserUuids,
    IEnumerable<string> DislikeUserUuids,
    IEnumerable<DeckDetailsSuggestionOutputDTO> Suggestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
