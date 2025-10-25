using BFlux;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.UIStore.States.Tags;

public record SetTagsAction(IReadOnlyList<Tag> Tags) : ImmutableAction<TagsState>
{
    public override TagsState Reduce(TagsState state)
    {
        return new TagsState(Tags);
    }
}