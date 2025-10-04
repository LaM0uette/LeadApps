using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class DeckSuggestionMappings
{
    public static DeckSuggestion ToEntity(this DeckSuggestionInputDTO dto)
    {
        return new DeckSuggestion
        {
            SuggestorId = dto.SuggestorId,
            Suggestor = null!, // set by EF
            DeckId = dto.DeckId,
            Deck = null!, // set by EF
            AddedCardIds = dto.AddedCardIds?.ToList() ?? [],
            RemovedCardIds = dto.RemovedCardIds?.ToList() ?? [],
            AddedEnergyIds = dto.AddedEnergyIds?.ToList() ?? [],
            RemovedEnergyIds = dto.RemovedEnergyIds?.ToList() ?? [],
            Likes = dto.Likes
        };
    }

    public static void UpdateEntity(this DeckSuggestion entity, DeckSuggestionInputDTO dto)
    {
        entity.SuggestorId = dto.SuggestorId;
        entity.DeckId = dto.DeckId;
        entity.AddedCardIds = dto.AddedCardIds?.ToList() ?? [];
        entity.RemovedCardIds = dto.RemovedCardIds?.ToList() ?? [];
        entity.AddedEnergyIds = dto.AddedEnergyIds?.ToList() ?? [];
        entity.RemovedEnergyIds = dto.RemovedEnergyIds?.ToList() ?? [];
        entity.Likes = dto.Likes;
    }
    
    public static DeckSuggestionOutputDTO ToOutput(this DeckSuggestion s)
    {
        return new DeckSuggestionOutputDTO(
            s.Id,
            s.Suggestor is null ? new UserInputDTO("", "", "", "") : new UserInputDTO(s.Suggestor.OAuthProvider, s.Suggestor.OAuthId, s.Suggestor.UserName, s.Suggestor.Email),
            s.Deck is null
                ? new DeckInputDTO(0, "", "", new List<int>(), new List<int>())
                : new DeckInputDTO(s.Deck.CreatorId, s.Deck.Name, s.Deck.Code, s.Deck.CardIds.ToList(), s.Deck.EnergyIds.ToList()),
            s.AddedCardIds.ToList(),
            s.RemovedCardIds.ToList(),
            s.AddedEnergyIds.ToList(),
            s.RemovedEnergyIds.ToList(),
            s.Likes,
            s.CreatedAt,
            s.UpdatedAt
        );
    }
}