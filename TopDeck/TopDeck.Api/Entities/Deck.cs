using System.ComponentModel.DataAnnotations;

namespace TopDeck.Api.Entities;

public class Deck
{
    public int Id { get; set; }
    
    public int CreatorId { get; set; }
    public required User Creator { get; set; }
    
    [MaxLength(100)]
    public required string Name { get; set; }
    
    [MaxLength(10)]
    public required string Code { get; set; }
    
    public ICollection<int> CardIds { get; set; } = [];
    public ICollection<int> EnergyIds { get; set; } = [];
    
    public int Likes { get; set; } = 0;
    
    public ICollection<DeckSuggestion> Suggestions { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}