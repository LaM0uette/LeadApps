using System.Linq.Expressions;
using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckItemMapper
{
    public static DeckItemOutputDTO MapToDTO(this Deck deck) => Expression.Compile().Invoke(deck);
    
    public static Expression<Func<Deck, DeckItemOutputDTO>> Expression => deck => new DeckItemOutputDTO(
        deck.Id,
        deck.Creator.Uuid.ToString(),
        deck.Name,
        deck.Code,
        deck.Cards
            .Where(card => card.IsHighlighted)
            .Select(card => new DeckItemCardOutputDTO(card.CollectionCode, card.CollectionNumber)),
        deck.EnergyIds,
        deck.DeckTags.Select(tag => tag.TagId),
        deck.Likes.Select(like => like.User.Uuid.ToString()),
        deck.Dislikes.Select(dislike => dislike.User.Uuid.ToString()),
        deck.CreatedAt
    );
    
    public static Deck ToEntity(this DeckItemInputDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Deck
        {
            CreatorId = dto.CreatorId,
            Creator = null!,
            Name = dto.Name,
            Code = string.Empty,

            Cards = dto.Cards.Select(c => new DeckCard
            {
                DeckId = 0,
                Deck = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber,
                IsHighlighted = c.IsHighlighted
            }).ToList(),

            EnergyIds = dto.EnergyIds.ToList(),

            DeckTags = dto.TagIds.Select(tagId => new DeckTag
            {
                DeckId = 0,
                Deck = null!,
                TagId = tagId,
                Tag = null!
            }).ToList(),

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(this Deck entity, DeckItemInputDTO dto)
    {
        entity.Name = dto.Name;

        entity.Cards = dto.Cards.Select(c => new DeckCard
        {
            DeckId = entity.Id,
            Deck = entity,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber,
            IsHighlighted = c.IsHighlighted
        }).ToList();

        entity.EnergyIds = dto.EnergyIds.ToList();

        entity.DeckTags = dto.TagIds.Select(id => new DeckTag
        {
            DeckId = entity.Id,
            Deck = entity,
            TagId = id,
            Tag = null!
        }).ToList();
    }
}