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
            dto.Cards.Select(c => new DeckDetailsCard(c.CollectionCode, c.CollectionNumber)).ToList(),
            dto.HighlightedCards.Select(c => new DeckDetailsCard(c.CollectionCode, c.CollectionNumber)).ToList(),
            dto.EnergyIds.ToList(),
            dto.TagIds.ToList(),
            dto.LikeUserUuids.ToList(),
            dto.DislikeUserUuids.ToList(),
            
            dto.Suggestions.Select(s => new DeckDetailsSuggestion(
                s.Id,
                s.SuggestorUuid,
                s.AddedCards.Select(ac => new DeckDetailsCard(ac.CollectionCode, ac.CollectionNumber)).ToList(),
                s.RemovedCards.Select(rc => new DeckDetailsCard(rc.CollectionCode, rc.CollectionNumber)).ToList(),
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
}