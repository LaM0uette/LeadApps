namespace TopDeck.Api.Entities;

public class DeckLike
{
    public int DeckId { get; set; }
    public required Deck Deck { get; set; }
    
    public int UserId { get; set; }
    public required User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}