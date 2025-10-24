namespace TopDeck.Domain.Models;

public record DeckDetails(
    int Id,
    string CreatorUui,
    string Name,
    string Code,
    ICollection<DeckDetailsCard> Cards,
    ICollection<int> EnergyIds,
    ICollection<int> TagIds,
    ICollection<string> LikeUserUuids,
    ICollection<string> DislikeUserUuids,
    ICollection<DeckDetailsSuggestion> Suggestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);