namespace TopDeck.Contracts.DTO;

public record DeckVoteInputDTO(
    int Id,
    string UserUuid,
    bool IsLike
);
