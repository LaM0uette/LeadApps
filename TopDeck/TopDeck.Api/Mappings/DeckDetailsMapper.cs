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
        deck.Cards
            .OrderBy(card => card.Id)
            .Select(card => new DeckDetailsCardOutputDTO(card.CollectionCode, card.CollectionNumber, card.IsHighlighted)),
        deck.EnergyIds,
        deck.DeckTags.Select(tag => tag.TagId),
        deck.Likes.Select(like => like.User.Uuid.ToString()),
        deck.Dislikes.Select(dislike => dislike.User.Uuid.ToString()),
        deck.Suggestions
            .OrderByDescending(s => s.UpdatedAt)
            .ThenByDescending(s => s.CreatedAt)
            .Select(s => new DeckDetailsSuggestionOutputDTO(
                s.Id,
                s.Suggestor.Uuid.ToString(),
                s.Suggestor.UserName,
                s.AddedCards.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)),
                s.RemovedCards.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)),
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
        // Normalize inputs: distinct by card identity and remove overlaps between added and removed
        var addedDistinct = dto.AddedCards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => g.First())
            .ToList();
        var removedDistinct = dto.RemovedCards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => g.First())
            .ToList();

        // Remove cards that appear in both lists (net-zero change) to avoid unique index violations
        var overlapKeys = new HashSet<(string Code, int Number)>(addedDistinct
            .Select(a => (a.CollectionCode, a.CollectionNumber))
            .Intersect(removedDistinct.Select(r => (r.CollectionCode, r.CollectionNumber))));
        if (overlapKeys.Count > 0)
        {
            addedDistinct = addedDistinct
                .Where(c => !overlapKeys.Contains((c.CollectionCode, c.CollectionNumber)))
                .ToList();
            removedDistinct = removedDistinct
                .Where(c => !overlapKeys.Contains((c.CollectionCode, c.CollectionNumber)))
                .ToList();
        }

        return new DeckSuggestion
        {
            SuggestorId = dto.SuggestorId,
            Suggestor = null!,
            DeckId = dto.DeckId,
            Deck = null!,

            AddedCards = addedDistinct.Select(c => new DeckSuggestionAddedCard
            {
                Suggestion = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber
            }).ToList(),

            RemovedCards = removedDistinct.Select(c => new DeckSuggestionRemovedCard
            {
                Suggestion = null!,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber
            }).ToList(),

            AddedEnergyIds = dto.AddedEnergyIds.Distinct().ToList(),
            RemovedEnergyIds = dto.RemovedEnergyIds.Distinct().ToList(),
        };
    }
    
    public static DeckDetailsSuggestionOutputDTO ToSuggestionDTO(this DeckSuggestion entity)
    {
        return new DeckDetailsSuggestionOutputDTO(
            entity.Id,
            entity.Suggestor?.Uuid.ToString() ?? string.Empty,
            entity.Suggestor?.UserName ?? string.Empty,
            entity.AddedCards?.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)) ?? [],
            entity.RemovedCards?.Select(c => new DeckDetailsCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)) ?? [],
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
        // Normalize inputs: distinct by card identity and remove overlaps between added and removed
        var addedDistinct = dto.AddedCards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => g.First())
            .ToList();
        var removedDistinct = dto.RemovedCards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => g.First())
            .ToList();

        var overlapKeys = new HashSet<(string Code, int Number)>(addedDistinct
            .Select(a => (a.CollectionCode, a.CollectionNumber))
            .Intersect(removedDistinct.Select(r => (r.CollectionCode, r.CollectionNumber))));
        if (overlapKeys.Count > 0)
        {
            addedDistinct = addedDistinct
                .Where(c => !overlapKeys.Contains((c.CollectionCode, c.CollectionNumber)))
                .ToList();
            removedDistinct = removedDistinct
                .Where(c => !overlapKeys.Contains((c.CollectionCode, c.CollectionNumber)))
                .ToList();
        }

        entity.AddedCards = addedDistinct.Select(c => new DeckSuggestionAddedCard
        {
            Suggestion = entity,
            DeckSuggestionId = entity.Id,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber
        }).ToList();

        entity.RemovedCards = removedDistinct.Select(c => new DeckSuggestionRemovedCard
        {
            Suggestion = entity,
            DeckSuggestionId = entity.Id,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber
        }).ToList();
        
        entity.AddedEnergyIds = dto.AddedEnergyIds.Distinct().ToList();
        entity.RemovedEnergyIds = dto.RemovedEnergyIds.Distinct().ToList();
    }
}
