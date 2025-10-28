using System.Net.Http.Json;
using System.Text.Json;

namespace TopDeck.Shared.Services;

public abstract class ApiService
{
    #region Statements

    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    protected ApiService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);

        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    #endregion

    #region Methods

    protected async Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<T>(requestUri, ct);
    }

    protected async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken ct = default)
    {
        using HttpResponseMessage response = await _http.PostAsJsonAsync(requestUri, payload, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    protected async Task<TResponse?> PutJsonAsync<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken ct = default)
    {
        using HttpResponseMessage response = await _http.PutAsJsonAsync(requestUri, payload, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    protected async Task<bool> DeleteAsync(string requestUri, CancellationToken ct = default)
    {
        using HttpResponseMessage response = await _http.DeleteAsync(requestUri, ct);
        return response.IsSuccessStatusCode;
    }

    #endregion
}
