using Helpers.Generators;
using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckItemService : IDeckItemService
{
    #region Statements

    private readonly IDeckItemRepository _repo;
    private readonly IUserRepository _users;
    private readonly IDeckSuggestionRepository _suggestions;

    public DeckItemService(IDeckItemRepository repo, IUserRepository users, IDeckSuggestionRepository suggestions)
    {
        _repo = repo;
        _users = users;
        _suggestions = suggestions;
    }

    #endregion

    #region IService
    
    public async Task<IReadOnlyList<DeckItemOutputDTO>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking().AsSplitQuery()
            .OrderByDescending(d => d.UpdatedAt).ThenByDescending(d => d.CreatedAt)
            .Skip(skip).Take(take)
            .Select(DeckItemMapper.Expression)
            .ToListAsync(ct);
    }
    
    public async Task<DeckItemOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking()
            .Where(d => d.Code == code)
            .Select(DeckItemMapper.Expression)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DeckItemOutputDTO> CreateAsync(DeckItemInputDTO dto, CancellationToken ct = default)
    {
        User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
        
        if (creator is null) 
            throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");

        ValidateDeckLimits(dto);

        Deck entity = dto.ToEntity();
        entity.Code = await GenerateUniqueCodeAsync(ct);
        
        if (string.IsNullOrEmpty(entity.Code))
            throw new InvalidOperationException("Failed to generate unique deck code");
        
        Deck created = await _repo.AddAsync(entity, ct);
        created.Creator = creator; // Set the navigation property for mapping
        return DeckItemMapper.MapToDTO(created);
    }

    public async Task<DeckItemOutputDTO?> UpdateAsync(int id, DeckItemInputDTO dto, CancellationToken ct = default)
    {
        // Load tracked entity with related collections to allow proper replacement
        Deck? existing = await _repo.DbSet
            .Include(d => d.Cards)
            .Include(d => d.DeckTags)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        
        if (existing is null)
            return null;

        User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
        if (creator is null)
            throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");

        if (existing.CreatorId != dto.CreatorId)
            throw new InvalidOperationException("Changing the creator of a deck is not allowed.");

        ValidateDeckLimits(dto);

        // Determine if the card composition changed (ignore highlight flag)
        var before = existing.Cards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .ToDictionary(g => g.Key, g => g.Count());
        var after = dto.Cards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .ToDictionary(g => g.Key, g => g.Count());
        bool cardsChanged = before.Count != after.Count || before.Any(kvp => !after.TryGetValue(kvp.Key, out int cnt) || cnt != kvp.Value);

        // Update scalar properties
        existing.Name = dto.Name;
        existing.EnergyIds = dto.EnergyIds.ToList();
        existing.UpdatedAt = DateTime.UtcNow;

        // Replace Cards: clear then add fresh items to avoid duplicates
        existing.Cards.Clear();
        foreach (DeckItemCardInputDTO c in dto.Cards)
        {
            existing.Cards.Add(new DeckCard
            {
                DeckId = existing.Id,
                Deck = existing,
                CollectionCode = c.CollectionCode,
                CollectionNumber = c.CollectionNumber,
                IsHighlighted = c.IsHighlighted
            });
        }

        // Replace Tags ensuring uniqueness
        existing.DeckTags.Clear();
        foreach (int tagId in dto.TagIds.Distinct())
        {
            existing.DeckTags.Add(new DeckTag
            {
                DeckId = existing.Id,
                Deck = existing,
                TagId = tagId,
                Tag = null!
            });
        }

        // If cards changed, clear all suggestions for this deck as they may be invalid now
        if (cardsChanged)
        {
            await _suggestions.DeleteByDeckIdAsync(existing.Id, ct);
        }

        Deck updated = await _repo.UpdateAsync(existing, ct);

        // Reload the deck with all navigation properties needed for mapping to avoid NREs
        Deck? reloaded = await _repo.DbSet
            .AsNoTracking()
            .Include(d => d.Creator)
            .Include(d => d.Cards)
            .Include(d => d.DeckTags)
            .Include(d => d.Likes).ThenInclude(l => l.User)
            .Include(d => d.Dislikes).ThenInclude(dl => dl.User)
            .FirstOrDefaultAsync(d => d.Id == updated.Id, ct);

        return reloaded is null ? DeckItemMapper.MapToDTO(updated) : DeckItemMapper.MapToDTO(reloaded);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _repo.DeleteAsync(id, ct);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _repo.CountAsync(ct);
    }

    #endregion

    #region Methods

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct = default)
    {
        string code;
        do
        {
            code = RandomStringGeneratorHelper.Generate(6);
        }
        while (await _repo.ExistsByCodeAsync(code, ct));

        return code;
    }

    private static void ValidateDeckLimits(DeckItemInputDTO dto)
    {
        int cardCount = dto.Cards.Count;
        int highlightedCount = dto.Cards.Count(c => c.IsHighlighted);

        if (cardCount > 20)
            throw new InvalidOperationException("A deck cannot contain more than 20 cards.");
        if (highlightedCount > 3)
            throw new InvalidOperationException("The cards highlighted are limited to 3.");
    }

    #endregion
}