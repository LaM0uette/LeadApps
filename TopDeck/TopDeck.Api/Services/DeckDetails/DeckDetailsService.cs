using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Mappings;
using TopDeck.Api.Repositories;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Services;

public class DeckDetailsService : IDeckDetailsService
{
    private readonly IDeckDetailsRepository _repo;

    public DeckDetailsService(IDeckDetailsRepository repo)
    {
        _repo = repo;
    }
    
    public async Task<DeckDetailsOutputDTO?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repo.DbSet
            .AsNoTracking()
            .Where(d => d.Code == code)
            .Select(DeckDetailsMapper.Expression)
            .FirstOrDefaultAsync(ct);
    }
}
