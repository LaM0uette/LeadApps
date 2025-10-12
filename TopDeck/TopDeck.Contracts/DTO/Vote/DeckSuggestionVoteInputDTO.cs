namespace TopDeck.Contracts.DTO;

public record DeckSuggestionVoteInputDTO(
    int Id,
    string UserUuid,
    bool IsLike
);
