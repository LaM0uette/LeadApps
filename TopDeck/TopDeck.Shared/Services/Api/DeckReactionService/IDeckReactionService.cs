using System.Threading;
using System.Threading.Tasks;

namespace TopDeck.Shared.Services;

public interface IDeckReactionService
{
    /// <summary>
    /// Toggle a like for a deck. When on == true, it will create a like (idempotent). When on == false, it will delete the like if present.
    /// </summary>
    Task<bool> LikeAsync(int deckId, int userId, bool on, CancellationToken ct = default);

    /// <summary>
    /// Toggle a dislike for a deck. When on == true, it will create a dislike (idempotent). When on == false, it will delete the dislike if present.
    /// </summary>
    Task<bool> DislikeAsync(int deckId, int userId, bool on, CancellationToken ct = default);
}