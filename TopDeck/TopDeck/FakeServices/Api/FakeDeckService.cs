using TopDeck.Contracts.DTO;
using TopDeck.Shared.Services;

namespace TopDeck.FakeServices;

/// <summary>
/// Fake in-memory implementation of IDeckService for the Blazor Server app.
/// Keeps data in a static list so it persists for the lifetime of the process.
/// </summary>
public class FakeDeckService : IDeckService
{
    private static readonly object _lock = new();
    private static readonly List<DeckOutputDTO> _decks = new();
    private static int _nextId = 1;

    public FakeDeckService()
    {
        if (_decks.Count == 0)
        {
            Seed();
        }
    }

    public Task<IReadOnlyList<DeckOutputDTO>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<DeckOutputDTO>)_decks.ToList());
        }
    }

    public Task<DeckOutputDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_decks.FirstOrDefault(d => d.Id == id));
        }
    }

    public Task<DeckOutputDTO> CreateAsync(DeckInputDTO dto, CancellationToken ct = default)
    {
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;
            DeckOutputDTO created = new DeckOutputDTO(
                Id: _nextId++,
                Creator: new UserOutputDTO(
                    Id: dto.CreatorId,
                    OAuthProvider: "local",
                    OAuthId: $"local|{dto.CreatorId}",
                    UserName: $"User {dto.CreatorId}",
                    Email: $"user{dto.CreatorId}@example.com",
                    CreatedAt: now
                ),
                Name: dto.Name,
                Code: dto.Code,
                CardIds: dto.CardIds.ToList(),
                EnergyIds: dto.EnergyIds.ToList(),
                Likes: new List<DeckLikeOutputDTO>(),
                Suggestions: new List<DeckSuggestionOutputDTO>(),
                CreatedAt: now,
                UpdatedAt: now
            );

            _decks.Add(created);
            return Task.FromResult(created);
        }
    }

    public Task<DeckOutputDTO?> UpdateAsync(int id, DeckInputDTO dto, CancellationToken ct = default)
    {
        lock (_lock)
        {
            int index = _decks.FindIndex(d => d.Id == id);
            if (index < 0) return Task.FromResult<DeckOutputDTO?>(null);

            DeckOutputDTO existing = _decks[index];
            DeckOutputDTO updated = existing with
            {
                Name = dto.Name,
                Code = dto.Code,
                CardIds = dto.CardIds.ToList(),
                EnergyIds = dto.EnergyIds.ToList(),
                UpdatedAt = DateTime.UtcNow
            };

            _decks[index] = updated;
            return Task.FromResult<DeckOutputDTO?>(updated);
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
        UserOutputDTO user = new UserOutputDTO(1, "local", "local|1", "Demo User", "demo@example.com", now);

        DeckOutputDTO deck1 = new DeckOutputDTO(
            Id: _nextId++,
            Creator: user,
            Name: "Starter Fire Deck",
            Code: "FIRE-001",
            CardIds: new List<int> { 1, 2, 3, 4, 5 },
            EnergyIds: new List<int> { 101, 102 },
            Likes: new List<DeckLikeOutputDTO>(),
            Suggestions: new List<DeckSuggestionOutputDTO>(),
            CreatedAt: now,
            UpdatedAt: now
        );

        DeckOutputDTO deck2 = new DeckOutputDTO(
            Id: _nextId++,
            Creator: user,
            Name: "Water Control",
            Code: "WATR-CTRL",
            CardIds: new List<int> { 10, 12, 14, 16, 18 },
            EnergyIds: new List<int> { 201 },
            Likes: new List<DeckLikeOutputDTO>(),
            Suggestions: new List<DeckSuggestionOutputDTO>(),
            CreatedAt: now,
            UpdatedAt: now
        );

        _decks.Add(deck1);
        _decks.Add(deck2);
    }
}
