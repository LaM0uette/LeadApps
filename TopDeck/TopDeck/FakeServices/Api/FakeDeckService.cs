using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

/// <summary>
/// Fake in-memory implementation of IDeckService for the Blazor Server app.
/// Keeps data in a static list so it persists for the lifetime of the process.
/// </summary>
public class FakeDeckService : IDeckService
{
    private static readonly object _lock = new();
    private static readonly List<Deck> _decks = new();
    private static int _nextId = 1;

    public FakeDeckService()
    {
        if (_decks.Count == 0)
        {
            Seed();
        }
    }

    public Task<IReadOnlyList<Deck>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<Deck>)_decks.ToList());
        }
    }

    public Task<Deck?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_decks.FirstOrDefault(d => d.Id == id));
        }
    }

    public Task<Deck> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;
            var user = new User(
                Id: dto.CreatorId,
                OAuthProvider: "local",
                OAuthId: $"local|{dto.CreatorId}",
                UserName: $"User {dto.CreatorId}",
                Email: $"user{dto.CreatorId}@example.com",
                CreatedAt: now
            );

            Deck created = new Deck(
                Id: _nextId++,
                Creator: user,
                Name: dto.Name,
                Code: dto.Code,
                CardIds: dto.CardIds.ToList(),
                EnergyIds: dto.EnergyIds.ToList(),
                Likes: new List<DeckLike>(),
                Suggestions: new List<DeckSuggestion>(),
                CreatedAt: now,
                UpdatedAt: now
            );

            _decks.Add(created);
            return Task.FromResult(created);
        }
    }

    public Task<Deck?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        lock (_lock)
        {
            int index = _decks.FindIndex(d => d.Id == id);
            if (index < 0) return Task.FromResult<Deck?>(null);

            Deck existing = _decks[index];
            Deck updated = existing with
            {
                Name = dto.Name,
                Code = dto.Code,
                CardIds = dto.CardIds.ToList(),
                EnergyIds = dto.EnergyIds.ToList(),
                UpdatedAt = DateTime.UtcNow
            };

            _decks[index] = updated;
            return Task.FromResult<Deck?>(updated);
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            bool removed = _decks.RemoveAll(d => d.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }

    private static void Seed()
    {
        DateTime now = DateTime.UtcNow;
        var user = new User(1, "local", "local|1", "Demo User", "demo@example.com", now);

        Deck deck1 = new Deck(
            Id: _nextId++,
            Creator: user,
            Name: "Starter Fire Deck",
            Code: "FIRE-001",
            CardIds: new List<int> { 1, 2, 3, 4, 5 },
            EnergyIds: new List<int> { 101, 102 },
            Likes: new List<DeckLike>(),
            Suggestions: new List<DeckSuggestion>(),
            CreatedAt: now,
            UpdatedAt: now
        );

        Deck deck2 = new Deck(
            Id: _nextId++,
            Creator: user,
            Name: "Water Control",
            Code: "WATR-CTRL",
            CardIds: new List<int> { 10, 12, 14, 16, 18 },
            EnergyIds: new List<int> { 201 },
            Likes: new List<DeckLike>(),
            Suggestions: new List<DeckSuggestion>(),
            CreatedAt: now,
            UpdatedAt: now
        );

        _decks.Add(deck1);
        _decks.Add(deck2);
    }
}
