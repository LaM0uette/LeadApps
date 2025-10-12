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
            s.Suggestor.UserName,
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
    
    
    public static DeckDetailsSuggestionOutputDTO MapToDTO(this DeckSuggestion suggestion) => ToSuggestionDTO(suggestion);

    public static DeckSuggestion ToSuggestionEntity(DeckSuggestionInputDTO dto)
    {
        return new DeckSuggestion
        {
            SuggestorId = dto.SuggestorId,
            Suggestor = null!,
            DeckId = dto.DeckId,
            Deck = null!,

            AddedCards = dto.AddedCards.Select(c => new DeckSuggestionAddedCard
            {
                Suggestion = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber
            }).ToList(),

            RemovedCards = dto.RemovedCards.Select(c => new DeckSuggestionRemovedCard
            {
                Suggestion = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber
            }).ToList(),

            AddedEnergyIds = dto.AddedEnergyIds.ToList(),
            RemovedEnergyIds = dto.RemovedEnergyIds.ToList(),
        };
    }
    
    public static DeckDetailsSuggestionOutputDTO ToSuggestionDTO(this DeckSuggestion entity)
    {
        return new DeckDetailsSuggestionOutputDTO(
            entity.Id,
            entity.Suggestor?.Uuid.ToString() ?? string.Empty,
            entity.Suggestor?.UserName ?? string.Empty,
            entity.AddedCards?.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber)) ?? [],
            entity.RemovedCards?.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber)) ?? [],
            entity.AddedEnergyIds ?? Enumerable.Empty<int>(),
            entity.RemovedEnergyIds ?? Enumerable.Empty<int>(),
            entity.Likes?.Select(l => l.User?.Uuid.ToString() ?? string.Empty) ?? [],
            entity.Dislikes?.Select(dl => dl.User?.Uuid.ToString() ?? string.Empty) ?? [],
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
    
    
    public static void UpdateEntity(this DeckSuggestion entity, DeckSuggestionInputDTO dto)
    {
        entity.AddedCards = dto.AddedCards.Select(c => new DeckSuggestionAddedCard
        {
            Suggestion = entity,
            DeckSuggestionId = entity.Id,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber
        }).ToList();

        entity.RemovedCards = dto.RemovedCards.Select(c => new DeckSuggestionRemovedCard
        {
            Suggestion = entity,
            DeckSuggestionId = entity.Id,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber
        }).ToList();
        
        entity.AddedEnergyIds = dto.AddedEnergyIds.ToList();
        entity.RemovedEnergyIds = dto.RemovedEnergyIds.ToList();
    }
}
