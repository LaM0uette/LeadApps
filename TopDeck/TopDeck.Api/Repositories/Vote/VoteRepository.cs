using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public class VoteRepository : IVoteRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public VoteRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion
    
    #region IRepository
    
    public async Task<DeckLike?> GetDeckLikeByIdAsync(int deckId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckLikes.FirstOrDefaultAsync(dl => dl.DeckId == deckId && dl.UserId == userId, ct);
    }
    
    public async Task<DeckSuggestionLike?> GetDeckSuggestionLikeByIdAsync(int suggestionId, int userId, CancellationToken ct = default)
    {
        return await _db.DeckSuggestionLikes.FirstOrDefaultAsync(dsl => dsl.DeckSuggestionId == suggestionId && dsl.UserId == userId, ct);
    }

    
    public async Task<DeckLike> AddDeckLikeAsync(DeckLike like, CancellationToken ct = default)
    {
        _db.DeckLikes.Add(like);
        await _db.SaveChangesAsync(ct);
        return like;
    }
    
    public async Task<bool> DeleteDeckLikeAsync(int deckId, int userId, CancellationToken ct = default)
    {
        DeckLike? existing = await _db.DeckLikes.FirstOrDefaultAsync(dl => dl.DeckId == deckId && dl.UserId == userId, ct);
        
        if (existing is null) 
            return false;
        
        _db.DeckLikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<DeckDislike> AddDeckDislikeAsync(DeckDislike dislike, CancellationToken ct = default)
    {
        _db.DeckDislikes.Add(dislike);
        await _db.SaveChangesAsync(ct);
        return dislike;
    }
    
    public async Task<bool> DeleteDeckDislikeAsync(int deckId, int userId, CancellationToken ct = default)
    {
        DeckDislike? existing = await _db.DeckDislikes.FirstOrDefaultAsync(dd => dd.DeckId == deckId && dd.UserId == userId, ct);
        
        if (existing is null) 
            return false;
        
        _db.DeckDislikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
    
    
    public async Task<DeckSuggestionLike> AddDeckSuggestionLikeAsync(DeckSuggestionLike like, CancellationToken ct = default)
    {
        _db.DeckSuggestionLikes.Add(like);
        await _db.SaveChangesAsync(ct);
        return like;
    }
    
    public async Task<bool> DeleteDeckSuggestionLikeAsync(int suggestionId, int userId, CancellationToken ct = default)
    {
        DeckSuggestionLike? existing = await _db.DeckSuggestionLikes.FirstOrDefaultAsync(dsl => dsl.DeckSuggestionId == suggestionId && dsl.UserId == userId, ct);
        
        if (existing is null) 
            return false;
        
        _db.DeckSuggestionLikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
    
    public async Task<DeckSuggestionDislike> AddDeckSuggestionDislikeAsync(DeckSuggestionDislike dislike, CancellationToken ct = default)
    {
        _db.DeckSuggestionDislikes.Add(dislike);
        await _db.SaveChangesAsync(ct);
        return dislike;
    }
    
    public async Task<bool> DeleteDeckSuggestionDislikeAsync(int suggestionId, int userId, CancellationToken ct = default)
    {
        DeckSuggestionDislike? existing = await _db.DeckSuggestionDislikes.FirstOrDefaultAsync(dsd => dsd.DeckSuggestionId == suggestionId && dsd.UserId == userId, ct);
        
        if (existing is null) 
            return false;
        
        _db.DeckSuggestionDislikes.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    #endregion
}