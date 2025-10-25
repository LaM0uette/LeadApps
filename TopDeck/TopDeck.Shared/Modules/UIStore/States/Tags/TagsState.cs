using BFlux;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.UIStore.States.Tags;

public record TagsState(
    IReadOnlyList<Tag> Tags
) : ImmutableState;