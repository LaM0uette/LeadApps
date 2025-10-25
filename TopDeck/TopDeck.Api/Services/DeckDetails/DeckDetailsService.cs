using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckDetailsService : IDeckDetailsService
{
    #region Statements

    private readonly IDeckDetailsRepository _repo;
    private readonly IDeckSuggestionRepository _repoSuggestions;
    private readonly IUserRepository _users;
    private readonly IDeckItemRepository _deckItems;

    public DeckDetailsService(IDeckDetailsRepository repo, IDeckSuggestionRepository repoSuggestions, IUserRepository users, IDeckItemRepository deckItems)
    {
        _repo = repo;
        _repoSuggestions = repoSuggestions;
        _users = users;
        _deckItems = deckItems;
    }

    #endregion

    #region IService

    public async Task<DeckDetailsOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking()
            .Where(d => d.Code == code)
            .Select(DeckDetailsMapper.Expression)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<DeckDetailsSuggestionOutputDTO> CreateSuggestionAsync(DeckSuggestionInputDTO dto, CancellationToken ct = default)
    {
        if (!await _repo.DbSet.AnyAsync(d => d.Id == dto.DeckId, ct))
            throw new InvalidOperationException($"Deck with ID {dto.DeckId} does not exist.");

        DeckSuggestion entity = DeckDetailsMapper.ToSuggestionEntity(dto);
        await _repoSuggestions.AddAsync(entity, ct);
        // Reload with relations to ensure Suggestor and related data are populated for the DTO
        DeckSuggestion? loaded = await _repoSuggestions.GetByIdAsync(entity.Id, includeRelations: true, ct);
        return (loaded ?? entity).ToSuggestionDTO();
    }
    
    public async Task<DeckDetailsSuggestionOutputDTO?> UpdateSuggestionAsync(int id, DeckSuggestionInputDTO dto, CancellationToken ct = default)
    {
        DeckSuggestion? existing = await _repoSuggestions.GetByIdAsync(id, includeRelations: false, ct);
        
        if (existing is null) 
            return null;
        
        if (await _users.GetByIdAsync(dto.SuggestorId, ct) is null)
            throw new InvalidOperationException($"Suggestor with id {dto.SuggestorId} not found");
        if (await _deckItems.GetByIdAsync(dto.DeckId, false, ct) is null)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");
        
        existing.UpdateEntity(dto);
        DeckSuggestion updated = await _repoSuggestions.UpdateAsync(existing, ct);
        return updated.ToSuggestionDTO();
    }
    
    public async Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default)
    {
        return await _repoSuggestions.DeleteAsync(id, ct);
    }

    #endregion
}
