using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Mappings;

public static class DeckItemMapper
{
    public static IReadOnlyList<DeckItem> ToDomain(this IEnumerable<DeckItemOutputDTO> dtos) => dtos.Select(ToDomain).ToList();
    public static DeckItem ToDomain(this DeckItemOutputDTO dto)
    {
        return new DeckItem(
            dto.Id,
            dto.CreatorUuid,
            dto.Name,
            dto.Code,
            dto.HighlightedCards.Select(c => new DeckItemCard(c.CollectionCode, c.CollectionNumber)).ToList(),
            dto.EnergyIds.ToList(),
            dto.TagIds.ToList(),
            dto.LikeUserUuids.ToList(),
            dto.DislikeUserUuids.ToList(),
            dto.CreatedAt
        );
    }
}