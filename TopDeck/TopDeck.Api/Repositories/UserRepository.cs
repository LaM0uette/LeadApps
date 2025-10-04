using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Repositories.Interfaces;

namespace TopDeck.Api.Repositories;

public class UserRepository : IUserRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Repository
    
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<User?> GetByOAuthAsync(string provider, string oAuthId, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.OAuthProvider == provider && x.OAuthId == oAuthId, ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        User? existing = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (existing is null) return false;
        _db.Users.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    #endregion
}