using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckSuggestionService : IDeckSuggestionService
{
    #region Statements

    private readonly IDeckSuggestionRepository _suggestions;
    private readonly IUserRepository _users;
    private readonly IDeckRepository _decks;

    public DeckSuggestionService(IDeckSuggestionRepository suggestions, IUserRepository users, IDeckRepository decks)
    {
        _suggestions = suggestions;
        _users = users;
        _decks = decks;
    }

    #endregion

    #region IService

    public async Task<DeckSuggestionOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        DeckSuggestion? entity = await _suggestions.GetByIdAsync(id, includeRelations: true, ct);
        return entity is null ? null : entity.ToOutput();
    }

    public async Task<IReadOnlyList<DeckSuggestionOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<DeckSuggestion> list = await _suggestions.GetAllAsync(includeRelations: true, ct);
        return list.Select(s => s.ToOutput()).ToList();
    }

    public async Task<DeckSuggestionOutputDTO> CreateAsync(DeckSuggestionInputDTO dto, CancellationToken ct = default)
    {
        // Validate FKs
        if (await _users.GetByIdAsync(dto.SuggestorId, ct) is null)
            throw new InvalidOperationException($"Suggestor with id {dto.SuggestorId} not found");
        if (await _decks.GetByIdAsync(dto.DeckId, false, ct) is null)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");

        DeckSuggestion entity = dto.ToEntity();
        DeckSuggestion created = await _suggestions.AddAsync(entity, ct);
        DeckSuggestion withRelations = await _suggestions.GetByIdAsync(created.Id, includeRelations: true, ct) ?? created;
        return withRelations.ToOutput();
    }

    public async Task<DeckSuggestionOutputDTO?> UpdateAsync(int id, DeckSuggestionInputDTO dto, CancellationToken ct = default)
    {
        DeckSuggestion? existing = await _suggestions.GetByIdAsync(id, includeRelations: false, ct);
        if (existing is null) return null;

        if (await _users.GetByIdAsync(dto.SuggestorId, ct) is null)
            throw new InvalidOperationException($"Suggestor with id {dto.SuggestorId} not found");
        if (await _decks.GetByIdAsync(dto.DeckId, false, ct) is null)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");

        existing.UpdateEntity(dto);
        DeckSuggestion updated = await _suggestions.UpdateAsync(existing, ct);
        DeckSuggestion withRelations = await _suggestions.GetByIdAsync(updated.Id, includeRelations: true, ct) ?? updated;
        return withRelations.ToOutput();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _suggestions.DeleteAsync(id, ct);
    }

    #endregion
}