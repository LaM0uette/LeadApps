namespace TopDeck.Api.Entities;

public class DeckSuggestion
{
    public int Id { get; set; }
    
    public int SuggestorId { get; set; }
    public required User Suggestor { get; set; }
    
    public int DeckId { get; set; }
    public required Deck Deck { get; set; }
    
    public ICollection<int> AddedCardIds { get; set; } = [];
    public ICollection<int> RemovedCardIds { get; set; } = [];

    public ICollection<int> AddedEnergyIds { get; set; } = [];
    public ICollection<int> RemovedEnergyIds { get; set; } = [];
    
    public ICollection<DeckSuggestionLike> Likes { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}