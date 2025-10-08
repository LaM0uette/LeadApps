namespace TopDeck.Api.Entities;

public class DeckSuggestionDislike
{
    public int DeckSuggestionId { get; set; }
    public required DeckSuggestion Suggestion { get; set; }
    
    public int UserId { get; set; }
    public required User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
