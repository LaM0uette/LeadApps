namespace TopDeck.Api.Entities;

public class DeckCard
{
    public int Id { get; set; }

    public int DeckId { get; set; }
    public required Deck Deck { get; set; }

    public required string CollectionCode { get; set; }
    public int CollectionNumber { get; set; }
}
