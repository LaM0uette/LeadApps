using System.Linq.Expressions;
using TopDeck.Api.DTO;
using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckMapper
{
    public static List<DeckOutputDTO> MapToDTO(this IEnumerable<Deck> decks) => decks.Select(MapToDTO).ToList();
    public static DeckOutputDTO MapToDTO(this Deck deck) => Expression.Compile().Invoke(deck);
    public static Expression<Func<Deck, DeckOutputDTO>> Expression => deck => new DeckOutputDTO(
        deck.Id,
        deck.Creator.OAuthId,
        deck.Name,
        deck.Code,
        deck.Cards
            .Where(card => card.IsHighlighted)
            .Select(card => new DeckCardOutputDTO(card.CollectionCode, card.CollectionNumber)),
        deck.EnergyIds,
        deck.DeckTags.Select(tag => tag.Tag.Id),
        deck.Likes.Select(like => like.User.OAuthId),
        deck.Dislikes.Select(dislike => dislike.User.OAuthId),
        deck.CreatedAt
    );

    
    
    
    
    
    
    public static Deck ToEntity(this DeckInputDTO dto)
    {
        var allCards = (dto.Cards ?? Array.Empty<DeckCardInputDTO>())
            .Select(c => new DeckCard { Deck = null!, DeckId = 0, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber, IsHighlighted = c.IsHighlighted })
            .ToList();

        return new Deck
        {
            CreatorId = dto.CreatorId,
            Creator = null!, // set by EF from CreatorId
            Name = dto.Name,
            Code = string.Empty, // TODO: change this
            Cards = allCards,
            EnergyIds = dto.EnergyIds?.ToList() ?? [],
            DeckTags = (dto.TagIds ?? Array.Empty<int>()).Select(id => new DeckTag { Deck = null!, DeckId = 0, TagId = id, Tag = null! }).ToList()
        };
    }

    public static void UpdateEntity(this Deck entity, DeckInputDTO dto)
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