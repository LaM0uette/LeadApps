using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public class DeckDetailsRepository : IDeckDetailsRepository
{
    #region Statements

    private readonly ApplicationDbContext _db;

    public DeckDetailsRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    #endregion

    #region IRepository

    public DbSet<Deck> DbSet => _db.Decks;

    #endregion
}
