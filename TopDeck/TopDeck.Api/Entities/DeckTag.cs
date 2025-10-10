namespace TopDeck.Api.Entities;

public class DeckTag
{
    public int DeckId { get; set; }
    public required Deck Deck { get; set; }

    public int TagId { get; set; }
    public required Tag Tag { get; set; }
}
