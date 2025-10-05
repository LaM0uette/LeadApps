using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckService : IDeckService
{
    #region Statements

    private readonly IDeckRepository _decks;
    private readonly IUserRepository _users;

    public DeckService(IDeckRepository decks, IUserRepository users)
    {
        _decks = decks;
        _users = users;
    }

    #endregion

    #region IService
    
    public async Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Deck> list = await _decks.GetAllAsync(includeRelations: true, ct);
        return list.Select(d => d.ToOutput()).ToList();
    }

    public async Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        Deck? entity = await _decks.GetByIdAsync(id, includeRelations: true, ct);
        return entity?.ToOutput();
    }

    public async Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        // Validate creator exists
        User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
        if (creator is null) throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");

        Deck entity = dto.ToEntity();
        Deck created = await _decks.AddAsync(entity, ct);

        // Reload with relations for output
        Deck withRelations = await _decks.GetByIdAsync(created.Id, includeRelations: true, ct) ?? created;
        return withRelations.ToOutput();
    }

    public async Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        Deck? existing = await _decks.GetByIdAsync(id, includeRelations: false, ct);
        if (existing is null) return null;

        // Validate new creator id exists if changed
        if (existing.CreatorId != dto.CreatorId)
        {
            User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
            if (creator is null) throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");
        }

        existing.UpdateEntity(dto);
        Deck updated = await _decks.UpdateAsync(existing, ct);
        Deck withRelations = await _decks.GetByIdAsync(updated.Id, includeRelations: true, ct) ?? updated;
        return withRelations.ToOutput();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _decks.DeleteAsync(id, ct);
    }

    #endregion
}