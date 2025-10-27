using TopDeck.Domain.Models;

namespace TopDeck.Shared.Services;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
}