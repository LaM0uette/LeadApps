using System.Linq.Expressions;
using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckDetailsMapper
{
    public static DeckDetailsOutputDTO MapToDTO(this Deck deck) => Expression.Compile().Invoke(deck);

    public static Expression<Func<Deck, DeckDetailsOutputDTO>> Expression => deck => new DeckDetailsOutputDTO(
        deck.Id,
        deck.Creator.Uuid.ToString(),
        deck.Name,
        deck.Code,
        deck.Cards.Select(card => new DeckDetailsCardOutputDTO(card.CollectionCode, card.CollectionNumber)),
        deck.Cards.Where(card => card.IsHighlighted).Select(card => new DeckDetailsCardOutputDTO(card.CollectionCode, card.CollectionNumber)),
        deck.EnergyIds,
        deck.DeckTags.Select(tag => tag.TagId),
        deck.Likes.Select(like => like.User.Uuid.ToString()),
        deck.Dislikes.Select(dislike => dislike.User.Uuid.ToString()),
        deck.Suggestions.Select(s => new DeckDetailsSuggestionOutputDTO(
            s.Id,
            s.Suggestor.Uuid.ToString(),
            s.AddedCards.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber)),
            s.RemovedCards.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber)),
            s.AddedEnergyIds,
            s.RemovedEnergyIds,
            s.Likes.Select(l => l.User.Uuid.ToString()),
            s.Dislikes.Select(dl => dl.User.Uuid.ToString()),
            s.CreatedAt,
            s.UpdatedAt
        )),
        deck.CreatedAt,
        deck.UpdatedAt
    );
}
