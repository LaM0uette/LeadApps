using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Localizer;

public class JsonLocalizer : ILocalizer
{
    #region Statements

    public string CurrentCulture { get; private set; } = "en";
    
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    private Dictionary<string, string> _translations = new();

    public JsonLocalizer(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    #endregion

    #region ILocalizer

    public async Task InitializeAsync()
    {
        Uri uri = new(_navigationManager.Uri);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uri.Query);

        string? culture;
        if (query.TryGetValue("lng", out StringValues lng) && !string.IsNullOrWhiteSpace(lng))
        {
            culture = lng;
        }
        else
        {
            culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        CurrentCulture = culture ?? "en";
        await LoadAsync(CurrentCulture);
    }
    
    public string Localize(string key, string? fallback = null)
    {
        if (_translations.TryGetValue(key, out string? value))
            return value;

        return fallback ?? key;
    }

    
    private async Task LoadAsync(string culture)
    {
        try
        {
            string path = $"locales/{culture}.json";

            string json = await _httpClient.GetStringAsync(path);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch (Exception)
        {
            if (culture != "en")
            {
                await LoadAsync("en");
                CurrentCulture = "en";
            }
        }
    }

    #endregion
}