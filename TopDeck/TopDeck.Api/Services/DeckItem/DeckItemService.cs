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
    
    public async Task<IReadOnlyList<DeckItemOutputDTO>> GetPageAsync(DeckItemsFilterDTO filter, CancellationToken ct = default)
    {
        IQueryable<Deck> query = _repo.DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string s = filter.Search.Trim().ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(s) ||
                d.Code.ToLower().Contains(s) ||
                d.Creator.UserName.ToLower().Contains(s));
        }

        if (filter.TagIds is { Count: > 0 })
        {
            var tagSet = filter.TagIds.Distinct().ToList();
            query = query.Where(d => d.DeckTags.Any(dt => tagSet.Contains(dt.TagId)));
        }

        // Sorting
        IOrderedQueryable<Deck> ordered = filter.OrderBy switch
        {
            TopDeck.Contracts.Enums.DeckItemsOrderBy.Name => filter.Asc ? query.OrderBy(d => d.Name) : query.OrderByDescending(d => d.Name),
            TopDeck.Contracts.Enums.DeckItemsOrderBy.Likes => filter.Asc ? query.OrderBy(d => d.Likes.Count) : query.OrderByDescending(d => d.Likes.Count),
            // Recent (default) -> UpdatedAt
            _ => filter.Asc ? query.OrderBy(d => d.UpdatedAt) : query.OrderByDescending(d => d.UpdatedAt)
        };

        return await ordered
            .ThenByDescending(d => d.CreatedAt)
            .Skip(filter.Skip).Take(filter.Take)
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
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        
        if (existing is null)
            return null;

        // For updates, we do not rely on dto.CreatorId because the deck already has an owner.
        // This avoids 400 errors when the client doesn't have a valid user context.
        // Authorization should be handled via auth middleware in the future.
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
        // Update primitive collection in-place so EF Core change tracker persists it
        existing.EnergyIds.Clear();
        foreach (int eid in (dto.EnergyIds ?? Array.Empty<int>()).Distinct())
        {
            existing.EnergyIds.Add(eid);
        }
        existing.UpdatedAt = DateTime.UtcNow;

        // Replace Cards and Tags in DB by hard delete + re-insert to avoid any duplication issues
        var newCards = dto.Cards.Select(c => new DeckCard
        {
            DeckId = existing.Id,
            Deck = null!,
            CollectionCode = c.CollectionCode,
            CollectionNumber = c.CollectionNumber,
            IsHighlighted = c.IsHighlighted
        });
        await _repo.ReplaceDeckCardsAsync(existing.Id, newCards, ct);

        var newTags = dto.TagIds.Distinct().Select(tagId => new DeckTag
        {
            DeckId = existing.Id,
            Deck = null!,
            TagId = tagId,
            Tag = null!
        });
        await _repo.ReplaceDeckTagsAsync(existing.Id, newTags, ct);

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

    public async Task<int> GetTotalCountAsync(TopDeck.Contracts.DTO.DeckItemsFilterDTO filter, CancellationToken ct = default)
    {
        IQueryable<Deck> query = _repo.DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string s = filter.Search.Trim().ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(s) ||
                d.Code.ToLower().Contains(s) ||
                d.Creator.UserName.ToLower().Contains(s));
        }

        if (filter.TagIds is { Count: > 0 })
        {
            var tagSet = filter.TagIds.Distinct().ToList();
            query = query.Where(d => d.DeckTags.Any(dt => tagSet.Contains(dt.TagId)));
        }

        return await query.CountAsync(ct);
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
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // 1) Exactly 20 cards required
        int cardCount = dto.Cards.Count;
        if (cardCount != 20)
            throw new InvalidOperationException("A deck must contain exactly 20 cards.");

        // 2) Max 2 copies per unique card (by CollectionCode + CollectionNumber)
        var duplicates = dto.Cards
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => new { g.Key.CollectionCode, g.Key.CollectionNumber, Count = g.Count() })
            .Where(x => x.Count > 2)
            .ToList();
        if (duplicates.Count > 0)
        {
            string details = string.Join(", ", duplicates.Select(d => $"{d.CollectionCode}:{d.CollectionNumber} x{d.Count}"));
            throw new InvalidOperationException($"A deck cannot contain more than 2 copies of the same card. Offenders: {details}");
        }

        // 3) Highlighted cards constraints: max 3, unique, and must exist among deck cards
        var highlighted = dto.Cards.Where(c => c.IsHighlighted).ToList();
        if (highlighted.Count > 3)
            throw new InvalidOperationException("The cards highlighted are limited to 3.");

        var highlightedGroups = highlighted
            .GroupBy(c => new { c.CollectionCode, c.CollectionNumber })
            .Select(g => new { g.Key.CollectionCode, g.Key.CollectionNumber, Count = g.Count() })
            .ToList();
        if (highlightedGroups.Any(g => g.Count > 1))
            throw new InvalidOperationException("Highlighted cards must be unique (at most one highlight per card).");

        // Ensure each highlighted card exists in deck (it does by construction as it's part of dto.Cards),
        // but keep check in case model changes
        var deckKeys = dto.Cards.Select(c => new { c.CollectionCode, c.CollectionNumber }).ToHashSet();
        if (highlightedGroups.Any(h => !deckKeys.Contains(new { h.CollectionCode, h.CollectionNumber })))
            throw new InvalidOperationException("Highlighted cards must be part of the deck cards.");
    }

    #endregion
}