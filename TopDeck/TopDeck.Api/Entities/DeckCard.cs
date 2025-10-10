using System.ComponentModel.DataAnnotations;

namespace TopDeck.Api.Entities;

public class DeckCard
{
    public int Id { get; set; }

    public int DeckId { get; set; }
    public required Deck Deck { get; set; }

    [MaxLength(10)]
    public required string CollectionCode { get; set; }
    public int CollectionNumber { get; set; }

    public bool IsHighlighted { get; set; }
}
