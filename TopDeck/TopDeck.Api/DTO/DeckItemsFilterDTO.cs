using TopDeck.Contracts.Enums;

namespace TopDeck.Api.DTO;

public class DeckItemsFilterDTO
{
    public int Skip { get; init; }
    public int Take { get; init; }
    public string? Search { get; init; }
    public IReadOnlyList<int>? TagIds { get; init; }
    public DeckItemsOrderBy? OrderBy { get; init; }
    public bool Asc { get; init; }
}
