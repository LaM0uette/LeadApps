using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Mappings;

/// <summary>
/// Mapping helpers to convert API OutputDTOs to Domain models.
/// Note: Only DTOs and domain models currently available are mapped.
/// </summary>
public static class DtoToDomainMappings
{
    // USER
    public static User ToDomain(this UserOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new User(
            dto.Id,
            dto.OAuthProvider,
            dto.OAuthId,
            dto.UserName,
            dto.CreatedAt
        );
    }

    // DECK
    public static Deck ToDomain(this DeckOutputDTOold dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // Base deck without likes/suggestions to break reference cycles
        var baseDeck = new Deck(
            dto.Id,
            dto.Creator.ToDomain(),
            dto.Name,
            dto.Code,
            (dto.Cards ?? Array.Empty<DeckCardOutputDTOold>()).Select(c => new DeckCard(c.CollectionCode, c.CollectionNumber, c.IsHighlighted)).ToList(),
            dto.EnergyIds?.ToList() ?? new List<int>(),
            (dto.Tags ?? Array.Empty<TagOutputDTOold>()).Select(t => new Tag(t.Id, t.Name, t.ColorHex)).ToList(),
            new List<DeckLike>(),
            new List<DeckDislike>(),
            new List<DeckSuggestion>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        // Map likes/suggestions with the deck context to avoid deep recursion
        var likes = (dto.Likes ?? Array.Empty<DeckLikeOutputDTO>())
            .Select(l => l.ToDomain(baseDeck))
            .ToList();
        
        var dislikes = (dto.Dislikes ?? Array.Empty<DeckDislikeOutputDTO>())
            .Select(d => d.ToDomain(baseDeck))
            .ToList();

        var suggestions = (dto.Suggestions ?? Array.Empty<DeckSuggestionOutputDTO>())
            .Select(s => s.ToDomain(baseDeck))
            .ToList();

        return baseDeck with { Likes = likes, Dislikes = dislikes, Suggestions = suggestions };
    }

    // DECK LIKE
    /// <summary>
    /// Standalone mapping (maps nested Deck fully). Prefer the overload with deckContext when available.
    /// </summary>
    public static DeckLike ToDomain(this DeckLikeOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new DeckLike(
            dto.Deck.ToDomain(),
            dto.User.ToDomain()
        );
    }

    /// <summary>
    /// Map a DeckLike using an existing Deck instance to avoid recursion.
    /// </summary>
    public static DeckLike ToDomain(this DeckLikeOutputDTO dto, Deck deckContext)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (deckContext is null) throw new ArgumentNullException(nameof(deckContext));
        return new DeckLike(
            deckContext,
            dto.User.ToDomain()
        );
    }
    
    // DECK DISLIKE
    public static DeckDislike ToDomain(this DeckDislikeOutputDTO dto, Deck deckContext)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (deckContext is null) throw new ArgumentNullException(nameof(deckContext));
        return new DeckDislike(
            deckContext,
            dto.User.ToDomain()
        );
    }

    // DECK SUGGESTION
    /// <summary>
    /// Standalone mapping (maps nested Deck fully). Prefer the overload with deckContext when available.
    /// </summary>
    public static DeckSuggestion ToDomain(this DeckSuggestionOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        var deck = dto.Deck.ToDomain();
        var suggestion = new DeckSuggestion(
            dto.Id,
            dto.Suggestor.ToDomain(),
            deck,
            (dto.AddedCards ?? Array.Empty<DeckCardOutputDTOold>()).Select(c => new DeckCard(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            (dto.RemovedCards ?? Array.Empty<DeckCardOutputDTOold>()).Select(c => new DeckCard(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            dto.AddedEnergyIds?.ToList() ?? new List<int>(),
            dto.RemovedEnergyIds?.ToList() ?? new List<int>(),
            new List<DeckSuggestionLike>(),
            new List<DeckSuggestionDislike>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        var likes = (dto.Likes ?? Array.Empty<DeckSuggestionLikeOutputDTO>())
            .Select(l => l.ToDomain(suggestion))
            .ToList();
        
        var dislikes = (dto.Dislikes ?? Array.Empty<DeckSuggestionDislikeOutputDTO>())
            .Select(d => d.ToDomain(suggestion))
            .ToList();

        return suggestion with { Likes = likes, Dislikes = dislikes };
    }

    /// <summary>
    /// Map a DeckSuggestion using an existing Deck instance to avoid recursion.
    /// </summary>
    public static DeckSuggestion ToDomain(this DeckSuggestionOutputDTO dto, Deck deckContext)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (deckContext is null) throw new ArgumentNullException(nameof(deckContext));
        var suggestion = new DeckSuggestion(
            dto.Id,
            dto.Suggestor.ToDomain(),
            deckContext,
            (dto.AddedCards ?? Array.Empty<DeckCardOutputDTOold>()).Select(c => new DeckCard(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            (dto.RemovedCards ?? Array.Empty<DeckCardOutputDTOold>()).Select(c => new DeckCard(c.CollectionCode, c.CollectionNumber, false)).ToList(),
            dto.AddedEnergyIds?.ToList() ?? new List<int>(),
            dto.RemovedEnergyIds?.ToList() ?? new List<int>(),
            new List<DeckSuggestionLike>(),
            new List<DeckSuggestionDislike>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        var likes = (dto.Likes ?? Array.Empty<DeckSuggestionLikeOutputDTO>())
            .Select(l => l.ToDomain(suggestion))
            .ToList();
        
        var dislikes = (dto.Dislikes ?? Array.Empty<DeckSuggestionDislikeOutputDTO>())
            .Select(d => d.ToDomain(suggestion))
            .ToList();

        return suggestion with { Likes = likes, Dislikes = dislikes };
    }

    // DECK SUGGESTION LIKE
    /// <summary>
    /// Standalone mapping (maps nested DeckSuggestion fully). Prefer the overload with suggestionContext when available.
    /// </summary>
    public static DeckSuggestionLike ToDomain(this DeckSuggestionLikeOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new DeckSuggestionLike(
            dto.DeckSuggestion.ToDomain(),
            dto.User.ToDomain()
        );
    }

    /// <summary>
    /// Map a DeckSuggestionLike using an existing DeckSuggestion instance to avoid recursion.
    /// </summary>
    public static DeckSuggestionLike ToDomain(this DeckSuggestionLikeOutputDTO dto, DeckSuggestion suggestionContext)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (suggestionContext is null) throw new ArgumentNullException(nameof(suggestionContext));
        return new DeckSuggestionLike(
            suggestionContext,
            dto.User.ToDomain()
        );
    }
    
    public static DeckSuggestionDislike ToDomain(this DeckSuggestionDislikeOutputDTO dto, DeckSuggestion suggestionContext)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (suggestionContext is null) throw new ArgumentNullException(nameof(suggestionContext));
        return new DeckSuggestionDislike(
            suggestionContext,
            dto.User.ToDomain()
        );
    }
}
