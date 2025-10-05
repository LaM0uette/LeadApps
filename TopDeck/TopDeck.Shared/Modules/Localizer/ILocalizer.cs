namespace Localizer;

public interface ILocalizer
{
    string CurrentCulture { get; }
    
    Task InitializeAsync();
    string Localize(string key, string? fallback = null);
}