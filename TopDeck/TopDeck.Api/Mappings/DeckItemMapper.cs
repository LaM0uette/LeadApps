using System.Linq.Expressions;
using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckItemMapper
{
    public static List<DeckItemOutputDTO> MapToDTO(this IEnumerable<Deck> decks) => decks.Select(MapToDTO).ToList();
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
        entity.CreatorId = dto.CreatorId;
        entity.Name = dto.Name;
        entity.Code = string.Empty; // TODO: change this

        var allCards = (dto.Cards ?? Array.Empty<DeckCardInputDTO>())
            .Select(c => new DeckCard { Deck = entity, DeckId = entity.Id, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber, IsHighlighted = c.IsHighlighted })
            .ToList();
        entity.Cards = allCards;

        entity.EnergyIds = dto.EnergyIds?.ToList() ?? [];

        entity.DeckTags = (dto.TagIds ?? Array.Empty<int>())
            .Select(id => new DeckTag { Deck = entity, DeckId = entity.Id, TagId = id, Tag = null! })
            .ToList();
    }

    // Shallow output to avoid circular references: empty Likes and Suggestions
    public static DeckOutputDTOold ToShallowOutput(this Deck entity)
    {
        return new DeckOutputDTOold(
            entity.Id,
            entity.Creator is null ? new UserOutputDTO(0, "", "", "", DateTime.MinValue) : entity.Creator.ToOutput(),
            entity.Name,
            entity.Code,
            entity.Cards.Select(c => new DeckCardOutputDTOold(c.CollectionCode, c.CollectionNumber, c.IsHighlighted)).ToList(),
            entity.EnergyIds.ToList(),
            entity.DeckTags.Select(dt => new TagOutputDTOold(dt.TagId, dt.Tag?.Name ?? string.Empty, dt.Tag?.ColorHex ?? string.Empty)).ToList(),
            new List<DeckLikeOutputDTO>(),
            new List<DeckDislikeOutputDTO>(),
            new List<DeckSuggestionOutputDTO>(),
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}