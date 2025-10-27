using System.Globalization;
using System.Text;
using System.Text.Json;
using TopDeck.Shared.Models.TCGP;
using TopDeck.Contracts.DTO;
using TopDeck.Shared.Mappings;

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

        // Call each per-type endpoint
        var pokemonsTask = GetJsonAsync<List<CardPokemonOutputDTO>>($"/cards/pokemon{urlParams}{loadThumbnailParam}", ct);
        var itemsTask = GetJsonAsync<List<CardItemOutputDTO>>($"/cards/item{urlParams}{loadThumbnailParam}", ct);
        var toolsTask = GetJsonAsync<List<CardToolOutputDTO>>($"/cards/tool{urlParams}{loadThumbnailParam}", ct);
        var supportersTask = GetJsonAsync<List<CardSupporterOutputDTO>>($"/cards/supporter{urlParams}{loadThumbnailParam}", ct);
        var fossilsTask = GetJsonAsync<List<CardFossilOutputDTO>>($"/cards/fossil{urlParams}{loadThumbnailParam}", ct);

        await Task.WhenAll(pokemonsTask!, itemsTask!, toolsTask!, supportersTask!, fossilsTask!);

        List<TCGPCard> all = new();

        if (pokemonsTask.Result is { } pkmDtos)
            all.AddRange(pkmDtos.ToDomain()); // returns List<TCGPPokemonCard> which is TCGPCard
        if (itemsTask.Result is { } itemDtos)
            all.AddRange(itemDtos.ToDomain());
        if (toolsTask.Result is { } toolDtos)
            all.AddRange(toolDtos.ToDomain());
        if (supportersTask.Result is { } supDtos)
            all.AddRange(supDtos.ToDomain());
        if (fossilsTask.Result is { } fosDtos)
            all.AddRange(fosDtos.ToDomain());

        return all;
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
        using HttpResponseMessage response = await _http.GetAsync(requestUri, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
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