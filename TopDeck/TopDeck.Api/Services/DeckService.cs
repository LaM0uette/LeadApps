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
    
    public async Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(bool includeAll = true, CancellationToken ct = default)
    {
        IReadOnlyList<Deck> list = await _decks.GetAllAsync(includeAll, ct);
        return list.MapToDTO();
    }

    public async Task<DeckOutputDTO?> GetByIdAsync(int id, bool includeAll = true, CancellationToken ct = default)
    {
        Deck? entity = await _decks.GetByIdAsync(id, includeAll, ct);
        return entity?.MapToDTO();
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
        
        Deck created = await _decks.AddAsync(entity, ct);
        return created.MapToDTO();
    }

    public async Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        Deck? existing = await _decks.GetByIdAsync(id, false, ct);
        
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
        Deck updated = await _decks.UpdateAsync(existing, ct);
        return updated.MapToDTO();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _decks.DeleteAsync(id, ct);
    }
    
    
    public async Task<IReadOnlyList<DeckOutputDTO>> GetDeckCardPageAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _decks.GetDbSet()
            .OrderBy(d => d.CreatedAt).ThenBy(d => d.Id)
            .Select(DeckMapper.Expression)
            .AsNoTracking().AsSplitQuery()
            .Skip(skip).Take(take)
            .ToListAsync(ct);
    }
    
    public async Task<DeckOutputDTO?> GetDeckCardByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _decks.GetDbSet()
            .Select(DeckMapper.Expression)
            .AsNoTracking()
            .FirstOrDefaultAsync(dto => dto.Code == code, ct);
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
        while (await _decks.ExistsByCodeAsync(code, ct));

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