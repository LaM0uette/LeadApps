namespace TopDeck.Api.Entities;

public class DeckSuggestionAddedCard
{
    public int Id { get; set; }

    public int DeckSuggestionId { get; set; }
    public required DeckSuggestion Suggestion { get; set; }

    public required string CollectionCode { get; set; }
    public int CollectionNumber { get; set; }
}
