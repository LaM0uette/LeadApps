using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories.Interfaces;

public interface IDeckRepository
{
    Task<IReadOnlyList<Deck>> GetAllAsync(bool includeRelations = false, CancellationToken ct = default);
    Task<Deck?> GetByIdAsync(int id, bool includeRelations = true, CancellationToken ct = default);
    Task<Deck> AddAsync(Deck deck, CancellationToken ct = default);
    Task<Deck> UpdateAsync(Deck deck, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
}