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

    public DeckItemService(IDeckItemRepository repo, IUserRepository users)
    {
        _repo = repo;
        _users = users;
    }

    #endregion

    #region IService
    
    public async Task<IReadOnlyList<DeckItemOutputDTO>> GetPageAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking().AsSplitQuery()
            .OrderBy(d => d.CreatedAt).ThenBy(d => d.Id)
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
        Deck? existing = await _repo.GetByIdAsync(id, false, ct);
        
        if (existing is null) 
            return null;

        User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
        
        if (creator is null) 
            throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");

        if (existing.CreatorId != dto.CreatorId)
            throw new InvalidOperationException("Changing the creator of a deck is not allowed.");

        ValidateDeckLimits(dto);

        existing.UpdateEntity(dto);
        Deck updated = await _repo.UpdateAsync(existing, ct);
        return DeckItemMapper.MapToDTO(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _repo.DeleteAsync(id, ct);
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