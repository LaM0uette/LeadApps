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

    public static DeckOutputDTO ToOutput(this Deck entity)
    {
        return new DeckOutputDTO(
            entity.Id,
            entity.Creator is null ? new UserInputDTO("", "", "", "") : new UserInputDTO(entity.Creator.OAuthProvider, entity.Creator.OAuthId, entity.Creator.UserName, entity.Creator.Email),
            entity.Name,
            entity.Code,
            entity.CardIds.ToList(),
            entity.EnergyIds.ToList(),
            entity.Likes,
            entity.Suggestions.Select(s => new DeckSuggestionInputDTO(
                s.SuggestorId,
                s.DeckId,
                s.AddedCardIds.ToList(),
                s.RemovedCardIds.ToList(),
                s.AddedEnergyIds.ToList(),
                s.RemovedEnergyIds.ToList(),
                s.Likes
            )).ToList(),
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}