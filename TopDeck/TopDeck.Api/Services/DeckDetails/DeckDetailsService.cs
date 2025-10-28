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

        // Normalize and bound the suggestion before persisting
        DeckSuggestionInputDTO normalized = await NormalizeSuggestionAsync(dto, ct);

        DeckSuggestion entity = DeckDetailsMapper.ToSuggestionEntity(normalized);
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
        
        // Normalize and bound the suggestion before updating
        DeckSuggestionInputDTO normalized = await NormalizeSuggestionAsync(dto, ct);

        existing.UpdateEntity(normalized);
        DeckSuggestion updated = await _repoSuggestions.UpdateAsync(existing, ct);
        return updated.ToSuggestionDTO();
    }
    
    public async Task<bool> DeleteSuggestionAsync(int id, CancellationToken ct = default)
    {
        return await _repoSuggestions.DeleteAsync(id, ct);
    }
 
    #endregion
 
    #region Methods
 
    private async Task<DeckSuggestionInputDTO> NormalizeSuggestionAsync(DeckSuggestionInputDTO dto, CancellationToken ct = default)
    {
        // Ensure suggestor and deck exist (Create path checks deck existence earlier)
        if (await _users.GetByIdAsync(dto.SuggestorId, ct) is null)
            throw new InvalidOperationException($"Suggestor with id {dto.SuggestorId} not found");

        // Load deck with cards to know current quantities (includeAll to load Cards)
        Deck? deck = await _deckItems.GetByIdAsync(dto.DeckId, includeAll: true, ct);
        if (deck is null)
            throw new InvalidOperationException($"Deck with id {dto.DeckId} not found");

        // Current quantities in deck (schema: 1 row per copy)
        var inDeck = deck.Cards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .ToDictionary(g => (g.Key.CollectionCode, g.Key.CollectionNumber), g => g.Count());

        // Count input lists and cap to 2 per card
        var addCounts = dto.AddedCards
            .GroupBy(c => (c.CollectionCode, c.CollectionNumber))
            .ToDictionary(g => g.Key, g => Math.Min(g.Count(), 2));
        var remCounts = dto.RemovedCards
            .GroupBy(c => (c.CollectionCode, c.CollectionNumber))
            .ToDictionary(g => g.Key, g => Math.Min(g.Count(), 2));

        // Net per card and cap by feasibility vs current deck (0..2 total per card)
        var keys = addCounts.Keys.Union(remCounts.Keys).ToList();
        var normalizedAdds = new List<DeckDetailsCardInputDTO>();
        var normalizedRems = new List<DeckDetailsCardInputDTO>();

        foreach (var key in keys)
        {
            int adds = addCounts.TryGetValue(key, out int a) ? a : 0;
            int rems = remCounts.TryGetValue(key, out int r) ? r : 0;
            int net = adds - rems; // >0 => add, <0 => remove

            inDeck.TryGetValue(key, out int current);

            if (net > 0)
            {
                int feasibleAdd = Math.Max(0, 2 - current);
                int qty = Math.Min(net, feasibleAdd);
                for (int i = 0; i < qty; i++)
                    normalizedAdds.Add(new DeckDetailsCardInputDTO(key.CollectionCode, key.CollectionNumber));
            }
            else if (net < 0)
            {
                int feasibleRem = Math.Min(-net, current);
                for (int i = 0; i < feasibleRem; i++)
                    normalizedRems.Add(new DeckDetailsCardInputDTO(key.CollectionCode, key.CollectionNumber));
            }
        }

        // Energies: ensure distinct
        var addedEnergies = dto.AddedEnergyIds?.Distinct().ToList() ?? new List<int>();
        var removedEnergies = dto.RemovedEnergyIds?.Distinct().ToList() ?? new List<int>();

        return new DeckSuggestionInputDTO(
            dto.SuggestorId,
            dto.DeckId,
            normalizedAdds,
            normalizedRems,
            addedEnergies,
            removedEnergies
        );
    }
 
    #endregion
}
