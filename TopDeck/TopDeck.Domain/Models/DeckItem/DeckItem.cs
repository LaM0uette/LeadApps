namespace TopDeck.Domain.Models;

public record DeckItem(
    int Id,
    string CreatorUui,
    string Name,
    string Code,
    ICollection<DeckItemCard> HighlightedCards,
    ICollection<int> EnergyIds,
    ICollection<int> TagIds,
    ICollection<string> LikeUserUuids,
    ICollection<string> DislikeUserUuids,
    DateTime CreatedAt
);