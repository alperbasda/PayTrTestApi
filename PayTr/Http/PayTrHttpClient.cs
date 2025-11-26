using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PayTr.Configuration;

namespace PayTr.Http;

/// <summary>
/// PayTR HTTP client implementasyonu
/// </summary>
public sealed class PayTrHttpClient : IPayTrHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public PayTrHttpClient(
        HttpClient httpClient,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Base URL'i ayarla
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        // JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Form-urlencoded POST isteği gönderir
    /// </summary>
    public async Task<TResponse> PostFormAsync<TResponse>(
        string relativeUrl,
        IDictionary<string, string> formData,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            throw new ArgumentException("Relative URL boş olamaz", nameof(relativeUrl));

        if (formData == null || formData.Count == 0)
            throw new ArgumentException("Form data boş olamaz", nameof(formData));

        // Form content oluştur
        using var content = new FormUrlEncodedContent(formData);

        // POST isteği gönder
        var response = await _httpClient.PostAsync(relativeUrl, content, ct);


        var t =await response.Content.ReadAsStringAsync();
        // Hata kontrolü
        response.EnsureSuccessStatusCode();

        // JSON'u parse et
        var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);

        if (result == null)
            throw new InvalidOperationException("Response null döndü");

        return result;
    }
}
