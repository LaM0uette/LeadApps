namespace TopDeck.Domain.Models;

public record DeckDetailsSuggestion(
    int Id,
    string SuggestorUuid,
    ICollection<DeckDetailsCard> AddedCards,
    ICollection<DeckDetailsCard> RemovedCards,
    ICollection<int> AddedEnergyIds,
    ICollection<int> RemovedEnergyIds,
    ICollection<string> LikeUserUuids,
    ICollection<string> DislikeUserUuids,
    DateTime CreatedAt,
    DateTime UpdatedAt
);