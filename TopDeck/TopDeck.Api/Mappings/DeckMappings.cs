using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckMappings
{
    public static Deck ToEntity(this DeckInputDTO dto)
    {
        return new Deck
        {
            CreatorId = dto.CreatorId,
            Creator = null!, // set by EF from CreatorId
            Name = dto.Name,
            Code = dto.Code,
            CardIds = dto.CardIds?.ToList() ?? [],
            EnergyIds = dto.EnergyIds?.ToList() ?? []
        };
    }

    public static void UpdateEntity(this Deck entity, DeckInputDTO dto)
    {
        entity.CreatorId = dto.CreatorId;
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.CardIds = dto.CardIds?.ToList() ?? [];
        entity.EnergyIds = dto.EnergyIds?.ToList() ?? [];
    }

    // Shallow output to avoid circular references: empty Likes and Suggestions
    public static DeckOutputDTO ToShallowOutput(this Deck entity)
    {
        return new DeckOutputDTO(
            entity.Id,
            entity.Creator is null ? new UserOutputDTO(0, "", "", "", "", DateTime.MinValue) : entity.Creator.ToOutput(),
            entity.Name,
            entity.Code,
            entity.CardIds.ToList(),
            entity.EnergyIds.ToList(),
            new List<DeckLikeOutputDTO>(),
            new List<DeckSuggestionOutputDTO>(),
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public static DeckOutputDTO ToOutput(this Deck entity)
    {
        // Build a shallow deck instance for nested references within likes to prevent recursion
        DeckOutputDTO shallowDeck = entity.ToShallowOutput();

        return new DeckOutputDTO(
            entity.Id,
            entity.Creator is null ? new UserOutputDTO(0, "", "", "", "", DateTime.MinValue) : entity.Creator.ToOutput(),
            entity.Name,
            entity.Code,
            entity.CardIds.ToList(),
            entity.EnergyIds.ToList(),
            entity.Likes.Select(l => new DeckLikeOutputDTO(
                shallowDeck,
                l.User is null ? new UserOutputDTO(0, "", "", "", "", DateTime.MinValue) : l.User.ToOutput()
            )).ToList(),
            entity.Suggestions.Select(s => s.ToOutput()).ToList(),
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}