using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckItemRepository
{
    DbSet<Deck> DbSet { get; }
    
    Task<IReadOnlyList<Deck>> GetAllAsync(bool includeAll = true, CancellationToken ct = default);
    Task<Deck?> GetByIdAsync(int id, bool includeAll = true, CancellationToken ct = default);
    Task<Deck> AddAsync(Deck deck, CancellationToken ct = default);
    Task<Deck> UpdateAsync(Deck deck, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    
    // Replace all DeckCards for a given deck by deleting existing and inserting the provided set
    Task ReplaceDeckCardsAsync(int deckId, IEnumerable<DeckCard> newCards, CancellationToken ct = default);
    
    // Replace all DeckTags for a given deck by deleting existing and inserting the provided set
    Task ReplaceDeckTagsAsync(int deckId, IEnumerable<DeckTag> newTags, CancellationToken ct = default);
}