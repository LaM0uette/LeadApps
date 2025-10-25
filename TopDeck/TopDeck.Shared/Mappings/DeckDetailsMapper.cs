using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Mappings;

public static class DeckDetailsMapper
{
    public static IReadOnlyList<DeckDetails> ToDomain(this IEnumerable<DeckDetailsOutputDTO> dtos) => dtos.Select(ToDomain).ToList();
    public static DeckDetails ToDomain(this DeckDetailsOutputDTO dto)
    {
        return new DeckDetails(
            dto.Id,
            dto.CreatorUuid,
            dto.Name,
            dto.Code,
            dto.Cards.Select(c => new DeckDetailsCard(c.CollectionCode, c.CollectionNumber, c.IsHighlighted)).ToList(),
            dto.EnergyIds.ToList(),
            dto.TagIds.ToList(),
            dto.LikeUserUuids.ToList(),
            dto.DislikeUserUuids.ToList(),
            
            dto.Suggestions.Select(s => new DeckDetailsSuggestion(
                s.Id,
                s.SuggestorUuid,
                s.SuggestorUsername,
                s.AddedCards.Select(ac => new DeckDetailsCard(ac.CollectionCode, ac.CollectionNumber, ac.IsHighlighted)).ToList(),
                s.RemovedCards.Select(rc => new DeckDetailsCard(rc.CollectionCode, rc.CollectionNumber, rc.IsHighlighted)).ToList(),
                s.AddedEnergyIds.ToList(),
                s.RemovedEnergyIds.ToList(),
                s.LikeUserUuids.ToList(),
                s.DislikeUserUuids.ToList(),
                s.CreatedAt,
                s.UpdatedAt
                )).ToList(),
            
            dto.CreatedAt,
            dto.UpdatedAt
        );
    }
    
    public static DeckDetailsSuggestion ToDomain(this DeckDetailsSuggestionOutputDTO dto)
    {
        return new DeckDetailsSuggestion(
            dto.Id,
            dto.SuggestorUuid,
            dto.SuggestorUsername,
            dto.AddedCards.Select(ac => new DeckDetailsCard(ac.CollectionCode, ac.CollectionNumber, ac.IsHighlighted)).ToList(),
            dto.RemovedCards.Select(rc => new DeckDetailsCard(rc.CollectionCode, rc.CollectionNumber, rc.IsHighlighted)).ToList(),
            dto.AddedEnergyIds.ToList(),
            dto.RemovedEnergyIds.ToList(),
            dto.LikeUserUuids.ToList(),
            dto.DislikeUserUuids.ToList(),
            dto.CreatedAt,
            dto.UpdatedAt
        );
    }
}