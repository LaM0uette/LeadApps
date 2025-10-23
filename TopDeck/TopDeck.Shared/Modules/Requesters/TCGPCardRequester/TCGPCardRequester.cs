using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TopDeck.Shared.Models.TCGP;

namespace TCGPCardRequester;

public class TCGPCardRequester : ITCGPCardRequester
{
    #region Statements

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://localhost:7057/"),
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    private readonly Dictionary<int, TCGPCard> _cache = new();

    #endregion

    #region Methods

    public async Task<List<TCGPCard>> GetAllTCGPCardsAsync(string? cultureOverride = null, bool loadThumbnail = false, CancellationToken ct = default)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string urlParams = $"?lng={cultureOverride ?? culture}";
        string loadThumbnailParam = $"&thumbnail={(loadThumbnail ? "true" : "false")}";
        
        List<TCGPCard>? tcgCards = await GetJsonAsync<List<TCGPCard>>($"/cards{urlParams}{loadThumbnailParam}", ct);
        
        return tcgCards ?? [];
    }

    public async Task<List<TCGPCard>> GetTCGPCardsByRequestAsync(TCGPCardsRequest deck, string? cultureOverride = null, bool loadThumbnail = false, CancellationToken ct = default)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string urlParams = $"?lng={cultureOverride ?? culture}";
        string loadThumbnailParam = $"&thumbnail={(loadThumbnail ? "true" : "false")}";
        
        List<TCGPCard> cards = await PostAsync<List<TCGPCard>>($"/cards/cards{urlParams}{loadThumbnailParam}", deck, ct);
        
        /*foreach (Card card in cards)
            _cache[card.Id] = card;*/
        
        return cards;
    }
    
    public async Task<List<int>> GetDeckPokemonTypesAsync(TCGPCardsRequest deck, CancellationToken ct = default)
    {
        List<int> types = await PostAsync<List<int>>("/cards/deckPokemonTypes", deck, ct);
        return types;
    }
    
    
    private async Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<T>(requestUri, ct);
    }

    private async Task<T> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
    {
        using StringContent? content = body is null ? null : new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
        
        using HttpResponseMessage response = await _http.PostAsync(path, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        T? deserialize = JsonSerializer.Deserialize<T>(json, _jsonOptions);
        return deserialize ?? throw new JsonException("Deserialization returned null.");
    }

    #endregion
}