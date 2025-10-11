using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckRepository
{
    DbSet<Deck> GetDbSet();
    
    Task<IReadOnlyList<Deck>> GetAllAsync(bool includeAll = true, CancellationToken ct = default);
    Task<Deck?> GetByIdAsync(int id, bool includeAll = true, CancellationToken ct = default);
    Task<Deck> AddAsync(Deck deck, CancellationToken ct = default);
    Task<Deck> UpdateAsync(Deck deck, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
}