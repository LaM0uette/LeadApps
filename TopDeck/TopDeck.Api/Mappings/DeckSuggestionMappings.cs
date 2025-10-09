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
            AddedCards = (dto.AddedCards ?? Array.Empty<DeckCardInputDTO>()).Select(c => new DeckSuggestionAddedCard { Suggestion = null!, DeckSuggestionId = 0, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber }).ToList(),
            RemovedCards = (dto.RemovedCards ?? Array.Empty<DeckCardInputDTO>()).Select(c => new DeckSuggestionRemovedCard { Suggestion = null!, DeckSuggestionId = 0, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber }).ToList(),
            AddedEnergyIds = dto.AddedEnergyIds?.ToList() ?? [],
            RemovedEnergyIds = dto.RemovedEnergyIds?.ToList() ?? []
        };
    }

    public static void UpdateEntity(this DeckSuggestion entity, DeckSuggestionInputDTO dto)
    {
        entity.SuggestorId = dto.SuggestorId;
        entity.DeckId = dto.DeckId;
        entity.AddedCards = (dto.AddedCards ?? Array.Empty<DeckCardInputDTO>()).Select(c => new DeckSuggestionAddedCard { Suggestion = entity, DeckSuggestionId = entity.Id, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber }).ToList();
        entity.RemovedCards = (dto.RemovedCards ?? Array.Empty<DeckCardInputDTO>()).Select(c => new DeckSuggestionRemovedCard { Suggestion = entity, DeckSuggestionId = entity.Id, CollectionCode = c.CollectionCode, CollectionNumber = c.CollectionNumber }).ToList();
        entity.AddedEnergyIds = dto.AddedEnergyIds?.ToList() ?? [];
        entity.RemovedEnergyIds = dto.RemovedEnergyIds?.ToList() ?? [];
    }

    // Shallow output to avoid circular references: empty Likes
    public static DeckSuggestionOutputDTO ToShallowOutput(this DeckSuggestion s)
    {
        return new DeckSuggestionOutputDTO(
            s.Id,
            s.Suggestor is null ? new UserOutputDTO(0, "", "", "", DateTime.MinValue) : s.Suggestor.ToOutput(),
            s.Deck is null ? new DeckOutputDTO(0, new UserOutputDTO(0, "", "", "", DateTime.MinValue), "", "", new List<DeckCardOutputDTO>(), new List<int>(), new List<DeckLikeOutputDTO>(), new List<DeckDislikeOutputDTO>(), new List<DeckSuggestionOutputDTO>(), DateTime.MinValue, DateTime.MinValue) : s.Deck.ToShallowOutput(),
            s.AddedCards.Select(c => new DeckCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            s.RemovedCards.Select(c => new DeckCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            s.AddedEnergyIds.ToList(),
            s.RemovedEnergyIds.ToList(),
            new List<DeckSuggestionLikeOutputDTO>(),
            new List<DeckSuggestionDislikeOutputDTO>(),
            s.CreatedAt,
            s.UpdatedAt
        );
    }
    
    public static DeckSuggestionOutputDTO ToOutput(this DeckSuggestion s)
    {
        DeckSuggestionOutputDTO shallow = s.ToShallowOutput();
        return new DeckSuggestionOutputDTO(
            s.Id,
            s.Suggestor is null ? new UserOutputDTO(0, "", "", "", DateTime.MinValue) : s.Suggestor.ToOutput(),
            s.Deck is null ? new DeckOutputDTO(0, new UserOutputDTO(0, "", "", "", DateTime.MinValue), "", "", new List<DeckCardOutputDTO>(), new List<int>(), new List<DeckLikeOutputDTO>(), new List<DeckDislikeOutputDTO>(), new List<DeckSuggestionOutputDTO>(), DateTime.MinValue, DateTime.MinValue) : s.Deck.ToShallowOutput(),
            s.AddedCards.Select(c => new DeckCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            s.RemovedCards.Select(c => new DeckCardOutputDTO(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            s.AddedEnergyIds.ToList(),
            s.RemovedEnergyIds.ToList(),
            s.Likes.Select(l => new DeckSuggestionLikeOutputDTO(
                shallow,
                l.User is null ? new UserOutputDTO(0, "", "", "", DateTime.MinValue) : l.User.ToOutput()
            )).ToList(),
            s.Dislikes.Select(l => new DeckSuggestionDislikeOutputDTO(
                shallow,
                l.User is null ? new UserOutputDTO(0, "", "", "", DateTime.MinValue) : l.User.ToOutput()
            )).ToList(),
            s.CreatedAt,
            s.UpdatedAt
        );
    }
}