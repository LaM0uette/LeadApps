using System.ComponentModel.DataAnnotations;

namespace TopDeck.Api.Entities;

public class Tag
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public required string Name { get; set; }
    
    [MaxLength(7)]
    public required string ColorHex { get; set; }

    public ICollection<DeckTag> DeckTags { get; set; } = [];
}
