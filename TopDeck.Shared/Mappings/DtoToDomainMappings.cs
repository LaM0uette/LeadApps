using System;
using System.Collections.Generic;
using System.Linq;
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
            dto.Email,
            dto.CreatedAt
        );
    }

    // DECK
    public static Deck ToDomain(this DeckOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // Base deck without likes/suggestions to break reference cycles
        var baseDeck = new Deck(
            dto.Id,
            dto.Creator.ToDomain(),
            dto.Name,
            dto.Code,
            dto.CardIds?.ToList() ?? new List<int>(),
            dto.EnergyIds?.ToList() ?? new List<int>(),
            new List<DeckLike>(),
            new List<DeckSuggestion>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        // Map likes/suggestions with the deck context to avoid deep recursion
        var likes = (dto.Likes ?? Array.Empty<DeckLikeOutputDTO>())
            .Select(l => l.ToDomain(baseDeck))
            .ToList();

        var suggestions = (dto.Suggestions ?? Array.Empty<DeckSuggestionOutputDTO>())
            .Select(s => s.ToDomain(baseDeck))
            .ToList();

        return baseDeck with { Likes = likes, Suggestions = suggestions };
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
            dto.AddedCardIds?.ToList() ?? new List<int>(),
            dto.RemovedCardIds?.ToList() ?? new List<int>(),
            dto.AddedEnergyIds?.ToList() ?? new List<int>(),
            dto.RemovedEnergyIds?.ToList() ?? new List<int>(),
            new List<DeckSuggestionLike>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        var likes = (dto.Likes ?? Array.Empty<DeckSuggestionLikeOutputDTO>())
            .Select(l => l.ToDomain(suggestion))
            .ToList();

        return suggestion with { Likes = likes };
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
            dto.AddedCardIds?.ToList() ?? new List<int>(),
            dto.RemovedCardIds?.ToList() ?? new List<int>(),
            dto.AddedEnergyIds?.ToList() ?? new List<int>(),
            dto.RemovedEnergyIds?.ToList() ?? new List<int>(),
            new List<DeckSuggestionLike>(),
            dto.CreatedAt,
            dto.UpdatedAt
        );

        var likes = (dto.Likes ?? Array.Empty<DeckSuggestionLikeOutputDTO>())
            .Select(l => l.ToDomain(suggestion))
            .ToList();

        return suggestion with { Likes = likes };
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
}
