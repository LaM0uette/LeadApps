namespace TopDeck.Shared.Models;

public sealed class OrderOption
{
    public string Key { get; }
    public string Label { get; }
    public bool DefaultAsc { get; }

    public OrderOption(string key, string label, bool defaultAsc = true)
    {
        Key = key;
        Label = label;
        DefaultAsc = defaultAsc;
    }
}