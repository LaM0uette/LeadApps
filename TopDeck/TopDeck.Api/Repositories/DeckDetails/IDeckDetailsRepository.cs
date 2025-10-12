using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Entities;

namespace TopDeck.Api.Repositories;

public interface IDeckDetailsRepository
{
    DbSet<Deck> DbSet { get; }
}
