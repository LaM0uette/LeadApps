using Helpers.Generators;
using Microsoft.EntityFrameworkCore;
using TopDeck.Api.DTO;
using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services.Interfaces;
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
    
    public async Task<IReadOnlyList<DeckOutputDTO>> GetDeckCardPageAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking().AsSplitQuery()
            .OrderBy(d => d.CreatedAt).ThenBy(d => d.Id)
            .Skip(skip).Take(take)
            .Select(DeckItemMapper.Expression)
            .ToListAsync(ct);
    }
    
    public async Task<DeckOutputDTO?> GetDeckCardByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking()
            .Where(d => d.Code == code)
            .Select(DeckItemMapper.Expression)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
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
        return created.MapToDTO();
    }

    public async Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        Deck? existing = await _repo.GetByIdAsync(id, false, ct);
        
        if (existing is null) 
            return null;

        if (existing.CreatorId != dto.CreatorId)
        {
            User? creator = await _users.GetByIdAsync(dto.CreatorId, ct);
            
            if (creator is null) 
                throw new InvalidOperationException($"Creator with id {dto.CreatorId} not found");
        }

        ValidateDeckLimits(dto);

        existing.UpdateEntity(dto);
        Deck updated = await _repo.UpdateAsync(existing, ct);
        return updated.MapToDTO();
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

    private static void ValidateDeckLimits(DeckInputDTO dto)
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