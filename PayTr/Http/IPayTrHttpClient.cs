namespace PayTr.Http;

/// <summary>
/// PayTR HTTP istekleri için arayüz
/// </summary>
public interface IPayTrHttpClient
{
    /// <summary>
    /// Form-urlencoded POST isteği gönderir ve JSON cevabı parse eder
    /// </summary>
    /// <typeparam name="TResponse">Response tipi</typeparam>
    /// <param name="relativeUrl">Endpoint (örn: /odeme, /odeme/iade)</param>
    /// <param name="formData">Form data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Parse edilmiş response</returns>
    Task<TResponse> PostFormAsync<TResponse>(
        string relativeUrl,
        IDictionary<string, string> formData,
        CancellationToken ct = default);

    /// <summary>
    /// Form-urlencoded POST isteği gönderir ve string olarak cevap döner
    /// </summary>
    /// <param name="relativeUrl">Endpoint (örn: /odeme/capi/list)</param>
    /// <param name="formData">Form data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response string</returns>
    Task<string> PostFormAsStringAsync(
        string relativeUrl,
        IDictionary<string, string> formData,
        CancellationToken ct = default);
}
